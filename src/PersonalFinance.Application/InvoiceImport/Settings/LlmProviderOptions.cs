namespace PersonalFinance.Application.InvoiceImport.Settings;

public sealed record class LlmProviderOptions
{
    public string Provider { get; init; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string Deployment { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 30;
}
