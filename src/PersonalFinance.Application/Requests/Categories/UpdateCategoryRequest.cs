namespace PersonalFinance.Application.Requests.Categories;

public sealed record class UpdateCategoryRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ColorHex { get; init; } = string.Empty;
    public Guid? ParentId { get; init; }
}
