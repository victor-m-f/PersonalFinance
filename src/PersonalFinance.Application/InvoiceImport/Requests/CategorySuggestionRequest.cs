namespace PersonalFinance.Application.InvoiceImport.Requests;

public sealed record class CategorySuggestionRequest
{
    public string VendorName { get; init; } = string.Empty;
    public string RawText { get; init; } = string.Empty;
    public IReadOnlyList<string> LineItems { get; init; } = Array.Empty<string>();
}
