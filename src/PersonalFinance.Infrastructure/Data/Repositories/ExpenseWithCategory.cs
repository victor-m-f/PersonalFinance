using PersonalFinance.Domain.Entities;

namespace PersonalFinance.Infrastructure.Data.Repositories;

internal sealed class ExpenseWithCategory
{
    public Expense Expense { get; init; } = default!;
    public Category? Category { get; init; }
}
