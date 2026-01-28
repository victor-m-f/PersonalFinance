namespace PersonalFinance.Application.InvoiceImport.Responses;

public sealed record class InvoiceInterpretationResult
{
    public string Json { get; init; } = string.Empty;
    public InvoiceInterpretationResponse Data { get; init; } = new();
}
