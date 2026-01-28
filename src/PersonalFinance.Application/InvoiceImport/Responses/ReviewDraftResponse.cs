namespace PersonalFinance.Application.InvoiceImport.Responses;

public sealed record class ReviewDraftResponse
{
    public Guid DocumentId { get; init; }
    public IReadOnlyList<ExpenseDraftItemResponse> Items { get; init; } = Array.Empty<ExpenseDraftItemResponse>();
}
