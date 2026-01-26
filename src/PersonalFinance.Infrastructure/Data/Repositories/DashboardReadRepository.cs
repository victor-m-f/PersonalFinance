using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.Dashboard.Abstractions;
using PersonalFinance.Application.Dashboard.Responses;

namespace PersonalFinance.Infrastructure.Data.Repositories;

public sealed class DashboardReadRepository : IDashboardReadRepository
{
    private readonly PersonalFinanceDbContext _dbContext;

    public DashboardReadRepository(PersonalFinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardTotalsResponse> GetTotalsAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct)
    {
        var query = _dbContext.Expenses.AsNoTracking()
            .Where(x => x.Date >= startDate && x.Date <= endDate);

        var total = await query.SumAsync(x => (double?)x.Amount, ct) ?? 0d;
        var count = await query.CountAsync(ct);

        return new DashboardTotalsResponse
        {
            TotalSpent = (decimal)total,
            ExpensesCount = count
        };
    }

    public async Task<IReadOnlyList<DashboardCategoryTotalResponse>> GetCategoryTotalsAsync(
        DateTime startDate,
        DateTime endDate,
        int take,
        CancellationToken ct)
    {
        var totals = await _dbContext.Expenses.AsNoTracking()
            .Where(x => x.Date >= startDate && x.Date <= endDate)
            .GroupBy(x => x.CategoryId)
            .Select(x => new
            {
                CategoryId = x.Key,
                TotalSpent = x.Sum(v => (double)v.Amount)
            })
            .OrderByDescending(x => x.TotalSpent)
            .Take(take)
            .ToListAsync(ct);

        var categoryIds = totals
            .Where(x => x.CategoryId.HasValue)
            .Select(x => x.CategoryId!.Value)
            .ToList();

        var categoryMap = await _dbContext.Categories.AsNoTracking()
            .Where(x => categoryIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Name, ColorHex = x.Color.Value })
            .ToDictionaryAsync(x => x.Id, ct);

        var results = totals.Select(item =>
        {
            var categoryId = item.CategoryId;
            categoryMap.TryGetValue(categoryId ?? Guid.Empty, out var category);
            return new DashboardCategoryTotalResponse
            {
                CategoryId = categoryId,
                CategoryName = category?.Name,
                CategoryColorHex = category?.ColorHex,
                TotalSpent = (decimal)item.TotalSpent
            };
        }).ToList();

        return results;
    }

    public async Task<IReadOnlyList<DashboardRecentExpenseResponse>> GetRecentExpensesAsync(
        DateTime startDate,
        DateTime endDate,
        int take,
        CancellationToken ct)
    {
        var query = _dbContext.Expenses.AsNoTracking()
            .Where(x => x.Date >= startDate && x.Date <= endDate)
            .GroupJoin(
                _dbContext.Categories.AsNoTracking(),
                expense => expense.CategoryId,
                category => category.Id,
                (expense, categories) => new { expense, categories })
            .SelectMany(
                x => x.categories.DefaultIfEmpty(),
                (x, category) => new
                {
                    x.expense,
                    CategoryName = category == null ? null : category.Name,
                    CategoryColorHex = category == null ? null : category.Color.Value
                })
            .OrderByDescending(x => x.expense.Date)
            .ThenByDescending(x => x.expense.Id)
            .Take(take)
            .Select(x => new DashboardRecentExpenseResponse
            {
                Id = x.expense.Id,
                Date = x.expense.Date,
                Amount = x.expense.Amount,
                Description = x.expense.Description,
                CategoryId = x.expense.CategoryId,
                CategoryName = x.CategoryName,
                CategoryColorHex = x.CategoryColorHex
            });

        return await query.ToListAsync(ct);
    }
}
