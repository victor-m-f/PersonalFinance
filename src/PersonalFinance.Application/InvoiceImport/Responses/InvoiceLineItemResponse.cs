namespace PersonalFinance.Application.InvoiceImport.Responses;

public sealed record class InvoiceLineItemResponse
{
    public string Description { get; init; } = string.Empty;
    public decimal? Quantity { get; init; }
    public decimal? UnitPrice { get; init; }
    public decimal? TotalPrice { get; init; }
}
