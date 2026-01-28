namespace PersonalFinance.Application.InvoiceImport.Requests;

public sealed record class AddVendorCategoryRuleRequest
{
    public string Keyword { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public double Confidence { get; init; }
}
