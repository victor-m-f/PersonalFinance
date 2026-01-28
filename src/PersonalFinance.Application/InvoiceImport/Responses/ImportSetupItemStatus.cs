namespace PersonalFinance.Application.InvoiceImport.Responses;

public sealed record ImportSetupItemStatus
{
    public string Key { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? LanguageCode { get; init; }
    public string? ModelName { get; init; }
    public bool IsInstalled { get; init; }
    public bool IsRequired { get; init; }
    public string? Detail { get; init; }
    public string? ActionText { get; init; }
}
