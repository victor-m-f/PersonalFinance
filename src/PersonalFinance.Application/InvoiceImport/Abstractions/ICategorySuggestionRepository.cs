using PersonalFinance.Application.InvoiceImport.Responses;

namespace PersonalFinance.Application.InvoiceImport.Abstractions;

public interface ICategorySuggestionRepository
{
    public Task<IReadOnlyList<CategorySuggestionCategoryResponse>> GetCategoriesAsync(CancellationToken ct);
}
