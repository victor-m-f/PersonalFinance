namespace PersonalFinance.Application.InvoiceImport.Requests;

public sealed record class ImportDocumentRequest
{
    public string SourceFilePath { get; init; } = string.Empty;
    public string OriginalFileName { get; init; } = string.Empty;
}
