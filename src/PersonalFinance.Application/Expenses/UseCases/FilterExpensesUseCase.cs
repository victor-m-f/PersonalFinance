using Microsoft.Extensions.Logging;
using PersonalFinance.Application.Abstractions.Paging;
using PersonalFinance.Application.Expenses.Abstractions;
using PersonalFinance.Application.Expenses.Requests;
using PersonalFinance.Application.Expenses.Responses;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.Expenses.UseCases;

public sealed class FilterExpensesUseCase
{
    private readonly IExpenseReadRepository _expenseReadRepository;
    private readonly ILogger<FilterExpensesUseCase> _logger;

    public FilterExpensesUseCase(
        IExpenseReadRepository expenseReadRepository,
        ILogger<FilterExpensesUseCase> logger)
    {
        _expenseReadRepository = expenseReadRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ExpenseListItemResponse>>> ExecuteAsync(
        FilterExpensesRequest request,
        CancellationToken ct)
    {
        var (descriptionSearch, sortBy, page) = GetClearFilters();

        _logger.LogInformation(
            "Filtering expenses. HasDateFilter {HasDateFilter}, HasAmountFilter {HasAmountFilter}, HasCategoryFilter {HasCategoryFilter}, HasDescriptionFilter {HasDescriptionFilter}, IsPaged {IsPaged}, SortBy {SortBy}, SortDescending {SortDescending}",
            request.StartDate.HasValue || request.EndDate.HasValue,
            request.MinAmount.HasValue || request.MaxAmount.HasValue,
            request.CategoryId.HasValue,
            descriptionSearch is not null,
            page is not null,
            sortBy,
            request.SortDescending);

        var result = await _expenseReadRepository.FilterAsync(
            request.StartDate,
            request.EndDate,
            request.MinAmount,
            request.MaxAmount,
            request.CategoryId,
            descriptionSearch,
            sortBy,
            request.SortDescending,
            page,
            ct);

        return Result<PagedResult<ExpenseListItemResponse>>.Success(result);

        (string? descriptionSearch, string? sortBy, PageRequest? page) GetClearFilters()
        {
            var descriptionSearch = request.DescriptionSearch?.Trim();
            if (string.IsNullOrWhiteSpace(descriptionSearch))
            {
                descriptionSearch = null;
            }

            var sortBy = request.SortBy?.Trim();
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                sortBy = null;
            }

            var page = request.Page is null ? null : PageRequest.Normalize(request.Page);

            return (descriptionSearch, sortBy, page);
        }
    }
}
