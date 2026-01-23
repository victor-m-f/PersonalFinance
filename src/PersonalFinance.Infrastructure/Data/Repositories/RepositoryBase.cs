using Microsoft.EntityFrameworkCore;
using PersonalFinance.Domain.Entities;

namespace PersonalFinance.Infrastructure.Data.Repositories;
public abstract class RepositoryBase<TEntity>
    where TEntity : EntityBase
{
    protected readonly PersonalFinanceDbContext DbContext;
    protected DbSet<TEntity> Set => DbContext.Set<TEntity>();

    protected RepositoryBase(PersonalFinanceDbContext dbContext)
    {
        DbContext = dbContext;
    }
}
