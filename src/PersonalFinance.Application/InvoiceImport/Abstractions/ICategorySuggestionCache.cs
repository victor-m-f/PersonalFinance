using PersonalFinance.Application.InvoiceImport.Responses;

namespace PersonalFinance.Application.InvoiceImport.Abstractions;

public interface ICategorySuggestionCache
{
    public Task<IReadOnlyList<CategorySuggestionCategoryResponse>> GetCategoriesAsync(CancellationToken ct);
}
