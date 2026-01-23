using PersonalFinance.Application.Categories.Abstractions;
using PersonalFinance.Domain.Entities;

namespace PersonalFinance.Infrastructure.Data.Repositories;

public sealed class CategoryRepository : RepositoryBase<Category>, ICategoryRepository
{
    public CategoryRepository(PersonalFinanceDbContext dbContext)
        : base(dbContext) { }

    public Task AddAsync(Category category, CancellationToken ct)
        => Set.AddAsync(category, ct).AsTask();
}
