namespace PersonalFinance.Application.InvoiceImport.Responses;

public sealed record ImportSetupProgress
{
    public string Key { get; init; } = string.Empty;
    public int Percent { get; init; }
    public string? Message { get; init; }
}
