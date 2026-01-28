namespace PersonalFinance.Application.InvoiceImport.Responses;

public sealed record class StoredDocumentResponse
{
    public string StoredFileName { get; init; } = string.Empty;
    public string FileExtension { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public string Hash { get; init; } = string.Empty;
}
