namespace PersonalFinance.Application.InvoiceImport.Requests;

public sealed record class ReviewDraftRequest
{
    public Guid DocumentId { get; init; }
    public IReadOnlyList<ExpenseDraftItemRequest> Items { get; init; } = Array.Empty<ExpenseDraftItemRequest>();
}
