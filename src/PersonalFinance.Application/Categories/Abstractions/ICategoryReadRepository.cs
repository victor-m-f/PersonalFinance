using PersonalFinance.Application.Abstractions.Paging;
using PersonalFinance.Application.Categories.Responses;

namespace PersonalFinance.Application.Categories.Abstractions;

public interface ICategoryReadRepository
{
    public Task<PagedResult<CategoryListItemResponse>> FilterAsync(
        Guid? parentId,
        string? name,
        string? sortBy,
        bool sortDescending,
        PageRequest? page,
        CancellationToken ct);
}
