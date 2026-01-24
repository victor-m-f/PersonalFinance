using PersonalFinance.Application.Abstractions.Paging;
using PersonalFinance.Application.Expenses.Responses;

namespace PersonalFinance.Application.Expenses.Abstractions;

public interface IExpenseReadRepository
{
    public Task<PagedResult<ExpenseListItemResponse>> FilterAsync(
        DateTime? startDate,
        DateTime? endDate,
        decimal? minAmount,
        decimal? maxAmount,
        Guid? categoryId,
        string? descriptionSearch,
        string? sortBy,
        bool sortDescending,
        PageRequest? page,
        CancellationToken ct);
}
