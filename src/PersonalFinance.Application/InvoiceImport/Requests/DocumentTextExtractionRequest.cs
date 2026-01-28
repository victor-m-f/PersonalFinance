namespace PersonalFinance.Application.InvoiceImport.Requests;

public sealed record class DocumentTextExtractionRequest
{
    public string StoredFileName { get; init; } = string.Empty;
    public string FileExtension { get; init; } = string.Empty;
    public Stream Content { get; init; } = Stream.Null;
}
