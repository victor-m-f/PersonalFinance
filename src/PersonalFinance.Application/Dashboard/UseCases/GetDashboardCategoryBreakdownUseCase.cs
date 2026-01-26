using FluentValidation;
using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Dashboard.Abstractions;
using PersonalFinance.Application.Dashboard.Requests;
using PersonalFinance.Application.Dashboard.Responses;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.Dashboard.UseCases;

public sealed class GetDashboardCategoryBreakdownUseCase
{
    private const int DefaultTopCount = 6;

    private readonly IDashboardReadRepository _readRepository;
    private readonly IValidator<DashboardPeriodRequest> _validator;
    private readonly ILogger<GetDashboardCategoryBreakdownUseCase> _logger;

    public GetDashboardCategoryBreakdownUseCase(
        IDashboardReadRepository readRepository,
        IValidator<DashboardPeriodRequest> validator,
        ILogger<GetDashboardCategoryBreakdownUseCase> logger)
    {
        _readRepository = readRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<DashboardCategoryTotalResponse>>> ExecuteAsync(
        DashboardPeriodRequest request,
        CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Result<IReadOnlyList<DashboardCategoryTotalResponse>>.Failure(
                Errors.ValidationError,
                validation.Errors.First().ErrorMessage);
        }

        var period = NormalizePeriod();

        _logger.LogInformation(
            "Dashboard category breakdown load. Start {StartDate}, End {EndDate}",
            period.Start,
            period.End);

        var result = await _readRepository.GetCategoryTotalsAsync(
            period.Start,
            period.End,
            DefaultTopCount,
            ct);

        return Result<IReadOnlyList<DashboardCategoryTotalResponse>>.Success(result);

        PeriodRange NormalizePeriod()
        {
            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                var startDate = request.StartDate.Value.Date;
                var endDate = request.EndDate.Value.Date;
                var endInclusive = endDate.AddDays(1).AddTicks(-1);
                return new PeriodRange(startDate, endInclusive, true);
            }

            var year = request.Year ?? DateTime.Today.Year;
            var month = request.Month ?? DateTime.Today.Month;
            var start = new DateTime(year, month, 1);
            var endDateOnly = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            var end = endDateOnly.AddDays(1).AddTicks(-1);
            return new PeriodRange(start, end, false);
        }
    }

    private sealed record class PeriodRange(
        DateTime Start,
        DateTime End,
        bool IsCustomRange);
}
