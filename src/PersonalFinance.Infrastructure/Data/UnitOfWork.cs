using PersonalFinance.Application.Abstractions;

namespace PersonalFinance.Infrastructure.Data;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly PersonalFinanceDbContext _dbContext;

    public UnitOfWork(PersonalFinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return _dbContext.SaveChangesAsync(ct);
    }
}
