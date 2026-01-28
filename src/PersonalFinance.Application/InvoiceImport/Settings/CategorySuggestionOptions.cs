namespace PersonalFinance.Application.InvoiceImport.Settings;

public sealed record class CategorySuggestionOptions
{
    public bool EnableLlm { get; init; } = true;
    public double MinHeuristicConfidence { get; init; } = 0.6d;
    public string SystemPrompt { get; init; } = string.Empty;
    public string UserPromptTemplate { get; init; } = string.Empty;
    public int CacheDurationSeconds { get; init; } = 300;
    public LlmProviderOptions Llm { get; init; } = new();
}
