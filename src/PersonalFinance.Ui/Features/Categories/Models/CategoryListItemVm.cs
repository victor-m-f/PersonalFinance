namespace PersonalFinance.Ui.Features.Categories.Models;

public sealed class CategoryListItemVm
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ColorHex { get; init; } = string.Empty;
    public Guid? ParentId { get; init; }
    public int Depth { get; init; }
    public string ParentHint { get; init; } = string.Empty;
}
