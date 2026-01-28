namespace PersonalFinance.Application.InvoiceImport.Responses;

public sealed record class ConfirmImportResponse
{
    public Guid DocumentId { get; init; }
    public int CreatedExpensesCount { get; init; }
    public IReadOnlyList<Guid> CreatedExpenseIds { get; init; } = Array.Empty<Guid>();
}
