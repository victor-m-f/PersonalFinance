using PersonalFinance.Application.InvoiceImport.Requests;
using PersonalFinance.Application.InvoiceImport.Responses;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.Abstractions;

public interface IDocumentTextExtractor
{
    public Task<Result<DocumentTextExtractionResponse>> ExtractAsync(
        DocumentTextExtractionRequest request,
        CancellationToken ct);
}
