namespace PersonalFinance.Application.InvoiceImport.Settings;

public sealed record class InvoiceInterpreterOptions
{
    public bool EnableLlm { get; init; }
    public string SystemPrompt { get; init; } = string.Empty;
    public string UserPromptTemplate { get; init; } = string.Empty;
    public LlmProviderOptions Llm { get; init; } = new();
}
