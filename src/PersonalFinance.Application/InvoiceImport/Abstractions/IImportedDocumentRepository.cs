using PersonalFinance.Domain.Entities;

namespace PersonalFinance.Application.InvoiceImport.Abstractions;

public interface IImportedDocumentRepository
{
    public Task AddAsync(ImportedDocument document, CancellationToken ct);
    public Task UpdateAsync(ImportedDocument document, CancellationToken ct);
    public Task<ImportedDocument?> GetByIdAsync(Guid id, CancellationToken ct);
    public Task<ImportedDocument?> GetByHashAsync(string hash, CancellationToken ct);
}
