namespace PersonalFinance.Application.InvoiceImport.Responses;

public sealed record class ExpenseDraftItemResponse
{
    public DateTime Date { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public double Confidence { get; init; }
}
