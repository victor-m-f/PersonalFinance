namespace PersonalFinance.Application.InvoiceImport.Responses;

public sealed record class InvoiceInterpretationResponse
{
    public string VendorName { get; init; } = string.Empty;
    public DateTime InvoiceDate { get; init; }
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public IReadOnlyList<InvoiceLineItemResponse>? LineItems { get; init; }
    public string Notes { get; init; } = string.Empty;
    public double Confidence { get; init; }
}
