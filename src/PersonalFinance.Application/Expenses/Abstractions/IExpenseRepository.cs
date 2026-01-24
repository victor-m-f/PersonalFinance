using PersonalFinance.Domain.Entities;

namespace PersonalFinance.Application.Expenses.Abstractions;

public interface IExpenseRepository
{
	public Task AddAsync(Expense expense, CancellationToken ct);
	public Task<Expense?> GetByIdAsync(Guid id, CancellationToken ct);
	public Task<bool> ExistsAsync(Guid id, CancellationToken ct);
	public Task<int> DeleteByIdAsync(Guid id, CancellationToken ct);
}
