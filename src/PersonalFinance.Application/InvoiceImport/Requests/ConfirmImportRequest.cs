namespace PersonalFinance.Application.InvoiceImport.Requests;

public sealed record class ConfirmImportRequest
{
    public Guid DocumentId { get; init; }
    public IReadOnlyList<ExpenseDraftItemRequest> Items { get; init; } = Array.Empty<ExpenseDraftItemRequest>();
}
