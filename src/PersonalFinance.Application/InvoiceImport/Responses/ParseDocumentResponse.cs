namespace PersonalFinance.Application.InvoiceImport.Responses;

public sealed record class ParseDocumentResponse
{
    public Guid DocumentId { get; init; }
    public bool IsOcrUsed { get; init; }
    public string RawText { get; init; } = string.Empty;
    public IReadOnlyList<ExpenseDraftItemResponse> Items { get; init; } = Array.Empty<ExpenseDraftItemResponse>();
    public double OverallConfidence { get; init; }
}
