using System.Globalization;
using FluentValidation;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Dashboard.Abstractions;
using PersonalFinance.Application.Dashboard.Requests;
using PersonalFinance.Application.Dashboard.Responses;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.Dashboard.UseCases;

public sealed class GetDashboardSummaryUseCase
{
    private readonly IDashboardReadRepository _readRepository;
    private readonly IValidator<DashboardPeriodRequest> _validator;
    private readonly ILogger<GetDashboardSummaryUseCase> _logger;

    public GetDashboardSummaryUseCase(
        IDashboardReadRepository readRepository,
        IValidator<DashboardPeriodRequest> validator,
        ILogger<GetDashboardSummaryUseCase> logger)
    {
        _readRepository = readRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<DashboardSummaryResponse>> ExecuteAsync(
        DashboardPeriodRequest request,
        CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Result<DashboardSummaryResponse>.Failure(
                Errors.ValidationError,
                validation.Errors.First().ErrorMessage);
        }

        var currentPeriod = NormalizePeriod();
        var previousPeriod = GetPreviousPeriod();

        _logger.LogInformation(
            "Dashboard summary load. Start {StartDate}, End {EndDate}",
            currentPeriod.Start,
            currentPeriod.End);

        var currentTotals = await _readRepository.GetTotalsAsync(currentPeriod.Start, currentPeriod.End, ct);
        var previousTotals = await _readRepository.GetTotalsAsync(previousPeriod.Start, previousPeriod.End, ct);

        var dailyAverage = currentPeriod.Days > 0
            ? currentTotals.TotalSpent / currentPeriod.Days
            : 0m;

        var changeText = BuildChangeText(currentTotals.TotalSpent, previousTotals.TotalSpent);
        decimal? changeAmount = previousTotals.TotalSpent <= 0m
            ? null
            : currentTotals.TotalSpent - previousTotals.TotalSpent;
        decimal? changePercent = previousTotals.TotalSpent <= 0m || changeAmount is null
            ? null
            : changeAmount.Value / previousTotals.TotalSpent * 100m;

        return Result<DashboardSummaryResponse>.Success(new DashboardSummaryResponse
        {
            TotalSpent = currentTotals.TotalSpent,
            DailyAverage = dailyAverage,
            ExpensesCount = currentTotals.ExpensesCount,
            PreviousTotalSpent = previousTotals.TotalSpent,
            PeriodChangeAmount = changeAmount,
            PeriodChangePercent = changePercent,
            PeriodChangeText = changeText
        });

        PeriodRange NormalizePeriod()
        {
            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                var startDate = request.StartDate.Value.Date;
                var endDate = request.EndDate.Value.Date;
                var days = (int)(endDate - startDate).TotalDays + 1;
                var endInclusive = endDate.AddDays(1).AddTicks(-1);
                return new PeriodRange(startDate, endInclusive, days, true);
            }

            var year = request.Year ?? DateTime.Today.Year;
            var month = request.Month ?? DateTime.Today.Month;
            var start = new DateTime(year, month, 1);
            var endDateOnly = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            var end = endDateOnly.AddDays(1).AddTicks(-1);
            var daysInMonth = DateTime.DaysInMonth(year, month);
            return new PeriodRange(start, end, daysInMonth, false);
        }

        PeriodRange GetPreviousPeriod()
        {
            if (!currentPeriod.IsCustomRange)
            {
                var previousMonth = currentPeriod.Start.AddMonths(-1);
                var start = new DateTime(previousMonth.Year, previousMonth.Month, 1);
                var endDateOnly = new DateTime(previousMonth.Year, previousMonth.Month, DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month));
                var end = endDateOnly.AddDays(1).AddTicks(-1);
                var daysInPreviousMonth = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
                return new PeriodRange(start, end, daysInPreviousMonth, false);
            }

            var currentStart = currentPeriod.Start.Date;
            var currentEndDate = currentPeriod.End.Date;
            var rangeDays = (int)(currentEndDate - currentStart).TotalDays + 1;
            var previousEnd = currentStart.AddDays(-1);
            var previousStart = previousEnd.AddDays(-rangeDays + 1);
            var previousEndInclusive = previousEnd.AddDays(1).AddTicks(-1);
            return new PeriodRange(previousStart, previousEndInclusive, rangeDays, true);
        }

        string BuildChangeText(decimal currentTotal, decimal previousTotal)
        {
            if (previousTotal <= 0m)
            {
                return "—";
            }

            var change = currentTotal - previousTotal;
            var sign = change >= 0m ? "+" : "-";
            var amountText = $"{sign}{FormatCurrency(Math.Abs(change))}";
            var percent = previousTotal == 0m ? 0m : change / previousTotal * 100m;
            var percentText = $"{sign}{Math.Abs(percent):0.0}%";
            return $"{amountText} ({percentText})";
        }

        string FormatCurrency(decimal amount)
        {
            return amount.ToString("C", CultureInfo.CurrentCulture);
        }
    }

    private sealed record class PeriodRange(
        DateTime Start,
        DateTime End,
        int Days,
        bool IsCustomRange);
}
