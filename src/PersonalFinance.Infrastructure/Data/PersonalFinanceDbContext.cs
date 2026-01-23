using Microsoft.EntityFrameworkCore;
using PersonalFinance.Domain.Entities;
using PersonalFinance.Infrastructure.Data.Configurations;

namespace PersonalFinance.Infrastructure.Data;

public sealed class PersonalFinanceDbContext : DbContext
{
    public DbSet<Category> Categories { get; private set; } = default!;
    public DbSet<Expense> Expenses { get; private set; } = default!;

    public PersonalFinanceDbContext(DbContextOptions<PersonalFinanceDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PersonalFinanceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        ExpenseConfiguration.ConfigureSaveChanges(ChangeTracker);
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}
