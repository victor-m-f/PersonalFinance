namespace PersonalFinance.Application.InvoiceImport.Responses;

public sealed record class CategorySuggestionCategoryResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? NormalizedName { get; init; }
}
