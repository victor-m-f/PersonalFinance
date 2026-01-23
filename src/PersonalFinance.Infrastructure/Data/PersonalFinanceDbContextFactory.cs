using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PersonalFinance.Infrastructure.Data;

public sealed class PersonalFinanceDbContextFactory : IDesignTimeDbContextFactory<PersonalFinanceDbContext>
{
    public PersonalFinanceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PersonalFinanceDbContext>();
        optionsBuilder.UseSqlite("Data Source=personalfinance.design.db");
        return new PersonalFinanceDbContext(optionsBuilder.Options);
    }
}
