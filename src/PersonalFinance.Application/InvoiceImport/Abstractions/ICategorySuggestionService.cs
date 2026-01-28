using PersonalFinance.Application.InvoiceImport.Requests;
using PersonalFinance.Application.InvoiceImport.Responses;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.InvoiceImport.Abstractions;

public interface ICategorySuggestionService
{
    public Task<Result<CategorySuggestionResponse>> SuggestAsync(
        CategorySuggestionRequest request,
        CancellationToken ct);
}
