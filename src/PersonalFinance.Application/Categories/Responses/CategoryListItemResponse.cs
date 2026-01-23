namespace PersonalFinance.Application.Categories.Responses;

public sealed record class CategoryListItemResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ColorHex { get; init; } = string.Empty;
    public Guid? ParentId { get; init; }
}
