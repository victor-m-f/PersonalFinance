using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.Abstractions.Paging;
using PersonalFinance.Application.Expenses.Abstractions;
using PersonalFinance.Application.Expenses.Responses;
using PersonalFinance.Infrastructure.Search;

namespace PersonalFinance.Infrastructure.Data.Repositories;

public sealed class ExpenseReadRepository : IExpenseReadRepository
{
    private readonly PersonalFinanceDbContext _dbContext;

    public ExpenseReadRepository(PersonalFinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ExpenseListItemResponse>> FilterAsync(
        DateTime? startDate,
        DateTime? endDate,
        decimal? minAmount,
        decimal? maxAmount,
        Guid? categoryId,
        string? descriptionSearch,
        string? sortBy,
        bool sortDescending,
        PageRequest? page,
        CancellationToken ct)
    {
        var expenses = _dbContext.Expenses.AsNoTracking();

        if (startDate.HasValue)
        {
            expenses = expenses.Where(x => x.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            expenses = expenses.Where(x => x.Date <= endDate.Value);
        }

        if (minAmount.HasValue)
        {
            expenses = expenses.Where(x => x.Amount >= minAmount.Value);
        }

        if (maxAmount.HasValue)
        {
            expenses = expenses.Where(x => x.Amount <= maxAmount.Value);
        }

        if (categoryId.HasValue)
        {
            expenses = expenses.Where(x => x.CategoryId == categoryId);
        }

        var normalizedSearch = TextSearchNormalizer.Normalize(descriptionSearch);
        if (normalizedSearch is not null)
        {
            expenses = expenses.Where(x => x.DescriptionSearch != null && EF.Functions.Like(x.DescriptionSearch, $"%{normalizedSearch}%"));
        }

        var totalCount = await expenses.CountAsync(ct);

        var query = expenses
            .GroupJoin(
                _dbContext.Categories.AsNoTracking(),
                expense => expense.CategoryId,
                category => category.Id,
                (expense, categories) => new { expense, categories })
            .SelectMany(
                x => x.categories.DefaultIfEmpty(),
                (x, category) => new ExpenseWithCategory
                {
                    Expense = x.expense,
                    Category = category
                });

        var orderedQuery = ApplySorting(query, sortBy, sortDescending);

        var projectedQuery = orderedQuery.Select(x => new ExpenseListItemResponse
        {
            Id = x.Expense.Id,
            Date = x.Expense.Date,
            Amount = x.Expense.Amount,
            Description = x.Expense.Description,
            CategoryId = x.Expense.CategoryId,
            CategoryName = x.Category == null ? null : x.Category.Name,
            CategoryColorHex = x.Category == null ? null : x.Category.Color.Value
        });

        if (page is null)
        {
            var items = await projectedQuery.ToListAsync(ct);
            return new PagedResult<ExpenseListItemResponse>(items, 1, totalCount, totalCount);
        }

        var normalizedPage = PageRequest.Normalize(page);
        var skip = (normalizedPage.PageNumber - 1) * normalizedPage.PageSize;

        var pagedItems = await projectedQuery
            .Skip(skip)
            .Take(normalizedPage.PageSize)
            .ToListAsync(ct);

        return new PagedResult<ExpenseListItemResponse>(
            pagedItems,
            normalizedPage.PageNumber,
            normalizedPage.PageSize,
            totalCount);
    }

    private static IQueryable<ExpenseWithCategory> ApplySorting(
        IQueryable<ExpenseWithCategory> query,
        string? sortBy,
        bool sortDescending)
    {
        if (string.Equals(sortBy, "Date", StringComparison.OrdinalIgnoreCase))
        {
            return sortDescending
                ? query.OrderByDescending(x => x.Expense.Date).ThenByDescending(x => x.Expense.Id)
                : query.OrderBy(x => x.Expense.Date).ThenBy(x => x.Expense.Id);
        }

        if (string.Equals(sortBy, "Amount", StringComparison.OrdinalIgnoreCase))
        {
            return sortDescending
                ? query.OrderByDescending(x => x.Expense.Amount).ThenByDescending(x => x.Expense.Id)
                : query.OrderBy(x => x.Expense.Amount).ThenBy(x => x.Expense.Id);
        }

        if (string.Equals(sortBy, "Description", StringComparison.OrdinalIgnoreCase))
        {
            return sortDescending
                ? query.OrderByDescending(x => x.Expense.DescriptionSearch).ThenByDescending(x => x.Expense.Id)
                : query.OrderBy(x => x.Expense.DescriptionSearch).ThenBy(x => x.Expense.Id);
        }

        if (string.Equals(sortBy, "CategoryName", StringComparison.OrdinalIgnoreCase))
        {
            return sortDescending
                ? query.OrderByDescending(x => x.Category == null ? null : x.Category.Name)
                    .ThenByDescending(x => x.Expense.Id)
                : query.OrderBy(x => x.Category == null ? null : x.Category.Name)
                    .ThenBy(x => x.Expense.Id);
        }

        return query.OrderByDescending(x => x.Expense.Date).ThenByDescending(x => x.Expense.Id);
    }

}
