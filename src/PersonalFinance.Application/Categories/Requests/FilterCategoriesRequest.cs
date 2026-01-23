using PersonalFinance.Application.Abstractions.Paging;

namespace PersonalFinance.Application.Categories.Requests;

public sealed record class FilterCategoriesRequest
{
    public Guid? ParentId { get; init; }
    public string? Name { get; init; }
    public PageRequest? Page { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
}
