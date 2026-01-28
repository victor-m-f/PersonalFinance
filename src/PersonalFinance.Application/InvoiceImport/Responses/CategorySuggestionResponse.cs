namespace PersonalFinance.Application.InvoiceImport.Responses;

public sealed record class CategorySuggestionResponse
{
    public Guid? SuggestedCategoryId { get; init; }
    public double Confidence { get; init; }
    public string Rationale { get; init; } = string.Empty;
}
