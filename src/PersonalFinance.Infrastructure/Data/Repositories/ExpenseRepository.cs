using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.Expenses.Abstractions;
using PersonalFinance.Domain.Entities;

namespace PersonalFinance.Infrastructure.Data.Repositories;

public sealed class ExpenseRepository : RepositoryBase<Expense>, IExpenseRepository
{
    public ExpenseRepository(PersonalFinanceDbContext dbContext)
        : base(dbContext) { }

    public Task AddAsync(Expense expense, CancellationToken ct)
        => Set.AddAsync(expense, ct).AsTask();

    public Task<Expense?> GetByIdAsync(Guid id, CancellationToken ct)
        => Set.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct)
        => Set.AnyAsync(x => x.Id == id, ct);

    public Task<int> DeleteByIdAsync(Guid id, CancellationToken ct)
        => Set.Where(x => x.Id == id).ExecuteDeleteAsync(ct);
}
