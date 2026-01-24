using PersonalFinance.Domain.Entities;

namespace PersonalFinance.Application.Categories.Abstractions;

public interface ICategoryRepository
{
	public Task AddAsync(Category category, CancellationToken ct);
	public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct);
	public Task<bool> ExistsAsync(Guid id, CancellationToken ct);
	public Task<Guid?> GetParentIdAsync(Guid id, CancellationToken ct);
	public Task<bool> HasChildrenAsync(Guid id, CancellationToken ct);
	public Task<bool> IsInUseByExpensesAsync(Guid categoryId, CancellationToken ct);
    public Task<int> DeleteByIdAsync(Guid id, CancellationToken ct);
}
