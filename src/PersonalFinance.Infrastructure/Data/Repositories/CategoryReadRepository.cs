using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.Abstractions.Paging;
using PersonalFinance.Application.Categories.Abstractions;
using PersonalFinance.Application.Categories.Responses;
using PersonalFinance.Domain.Entities;

namespace PersonalFinance.Infrastructure.Data.Repositories;

public sealed class CategoryReadRepository : ICategoryReadRepository
{
    private readonly PersonalFinanceDbContext _dbContext;

    public CategoryReadRepository(PersonalFinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<CategoryListItemResponse>> FilterAsync(
        Guid? parentId,
        bool includeAll,
        string? name,
        string? sortBy,
        bool sortDescending,
        PageRequest? page,
        CancellationToken ct)
    {
        var query = _dbContext.Categories.AsNoTracking();

        if (!includeAll)
        {
            if (parentId.HasValue)
            {
                query = query.Where(x => x.ParentId == parentId);
            }
            else
            {
                query = query.Where(x => x.ParentId == null);
            }
        }

        if (name is not null)
        {
            query = query.Where(x => EF.Functions.Like(x.Name, $"%{name}%"));
        }

        var totalCount = await query.CountAsync(ct);

        var orderedQuery = ApplySorting(query, sortBy, sortDescending);

        var projectedQuery = orderedQuery.Select(x => new CategoryListItemResponse
        {
            Id = x.Id,
            Name = x.Name,
            ColorHex = x.Color.Value,
            ParentId = x.ParentId
        });

        if (page is null)
        {
            var items = await projectedQuery.ToListAsync(ct);
            return new PagedResult<CategoryListItemResponse>(items, 1, totalCount, totalCount);
        }

        var normalizedPage = PageRequest.Normalize(page);
        var skip = (normalizedPage.PageNumber - 1) * normalizedPage.PageSize;

        var pagedItems = await projectedQuery
            .Skip(skip)
            .Take(normalizedPage.PageSize)
            .ToListAsync(ct);

        return new PagedResult<CategoryListItemResponse>(
            pagedItems,
            normalizedPage.PageNumber,
            normalizedPage.PageSize,
            totalCount);
    }

    private static IQueryable<Category> ApplySorting(
        IQueryable<Category> query,
        string? sortBy,
        bool sortDescending)
    {
        if (string.Equals(sortBy, "Name", StringComparison.OrdinalIgnoreCase))
        {
            if (sortDescending)
            {
                return query.OrderByDescending(x => x.Name).ThenByDescending(x => x.Id);
            }

            return query.OrderBy(x => x.Name).ThenBy(x => x.Id);
        }

        return query.OrderBy(x => x.Name).ThenBy(x => x.Id);
    }
}
