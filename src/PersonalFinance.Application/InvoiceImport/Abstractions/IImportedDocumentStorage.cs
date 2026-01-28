using PersonalFinance.Application.InvoiceImport.Responses;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Application.InvoiceImport.Abstractions;

public interface IImportedDocumentStorage
{
    public Task<Result<StoredDocumentResponse>> SaveAsync(
        string sourceFilePath,
        string originalFileName,
        CancellationToken ct);

    public Task<Result<Stream>> OpenReadAsync(
        string storedFileName,
        CancellationToken ct);
}
