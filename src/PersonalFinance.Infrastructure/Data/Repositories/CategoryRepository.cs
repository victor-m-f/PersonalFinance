using PersonalFinance.Application.Categories.Abstractions;
using PersonalFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace PersonalFinance.Infrastructure.Data.Repositories;

public sealed class CategoryRepository : RepositoryBase<Category>, ICategoryRepository
{
    public CategoryRepository(PersonalFinanceDbContext dbContext)
        : base(dbContext) { }

    public Task AddAsync(Category category, CancellationToken ct)
        => Set.AddAsync(category, ct).AsTask();

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct)
        => Set.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct)
        => Set.AnyAsync(x => x.Id == id, ct);

    public async Task<Guid?> GetParentIdAsync(Guid id, CancellationToken ct)
    {
        var parentId = await Set
            .Where(x => x.Id == id)
            .Select(x => x.ParentId)
            .FirstOrDefaultAsync(ct);

        return parentId;
    }

    public Task<bool> HasChildrenAsync(Guid id, CancellationToken ct)
        => Set.AnyAsync(x => x.ParentId == id, ct);

    public Task<bool> IsInUseByExpensesAsync(Guid categoryId, CancellationToken ct)
        => DbContext.Expenses.AnyAsync(x => x.CategoryId == categoryId, ct);

    public Task<int> DeleteByIdAsync(Guid id, CancellationToken ct)
        => Set.Where(x => x.Id == id).ExecuteDeleteAsync(ct);
}
