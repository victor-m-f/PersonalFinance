using Microsoft.EntityFrameworkCore;
using PersonalFinance.Application.InvoiceImport.Abstractions;
using PersonalFinance.Application.InvoiceImport.Responses;
using PersonalFinance.Infrastructure.Data;
using PersonalFinance.Infrastructure.Search;

namespace PersonalFinance.Infrastructure.InvoiceImport;

public sealed class CategorySuggestionRepository : ICategorySuggestionRepository
{
    private readonly PersonalFinanceDbContext _dbContext;

    public CategorySuggestionRepository(PersonalFinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CategorySuggestionCategoryResponse>> GetCategoriesAsync(CancellationToken ct)
    {
        var categories = await _dbContext.Categories
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.Name
            })
            .ToListAsync(ct);

        return categories.Select(x => new CategorySuggestionCategoryResponse
        {
            Id = x.Id,
            Name = x.Name,
            NormalizedName = TextSearchNormalizer.Normalize(x.Name)
        }).ToList();
    }
}
