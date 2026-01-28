using PersonalFinance.Domain.Entities;

namespace PersonalFinance.Application.InvoiceImport.Responses;

public sealed record class ImportDocumentResponse
{
    public Guid DocumentId { get; init; }
    public string Hash { get; init; } = string.Empty;
    public ImportedDocumentStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
