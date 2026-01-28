namespace PersonalFinance.Application.InvoiceImport.Requests;

public sealed record class InterpretInvoiceRequest
{
    public string RawText { get; init; } = string.Empty;
}
