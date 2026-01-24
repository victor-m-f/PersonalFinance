namespace PersonalFinance.Ui.Features.Categories.Models;

public sealed class CategoryLookupItemVm
{
    public Guid? Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Depth { get; init; }
    public string DisplayName { get; init; } = string.Empty;
}
