using PersonalFinance.Domain.Entities;

namespace PersonalFinance.Application.Categories.Abstractions;

public interface ICategoryRepository
{
	public Task AddAsync(Category category, CancellationToken ct);
}
