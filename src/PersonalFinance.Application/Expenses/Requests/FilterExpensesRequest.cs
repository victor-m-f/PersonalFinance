using PersonalFinance.Application.Abstractions.Paging;

namespace PersonalFinance.Application.Expenses.Requests;

public sealed record class FilterExpensesRequest
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public decimal? MinAmount { get; init; }
    public decimal? MaxAmount { get; init; }
    public Guid? CategoryId { get; init; }
    public string? DescriptionSearch { get; init; }
    public PageRequest? Page { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
}
