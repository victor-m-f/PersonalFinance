namespace PersonalFinance.Application.InvoiceImport.Requests;

public sealed record class ExpenseDraftItemRequest
{
    public DateTime Date { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public double Confidence { get; init; }
}
