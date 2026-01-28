namespace PersonalFinance.Application.InvoiceImport.Responses;

public sealed record class DocumentTextExtractionResponse
{
    public string RawText { get; init; } = string.Empty;
    public IReadOnlyList<string> PageTexts { get; init; } = Array.Empty<string>();
    public bool IsOcrUsed { get; init; }
}
