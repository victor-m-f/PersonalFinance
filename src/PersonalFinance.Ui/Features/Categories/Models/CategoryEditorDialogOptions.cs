namespace PersonalFinance.Ui.Features.Categories.Models;

public sealed record class CategoryEditorDialogOptions
{
    public Guid? Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ColorHex { get; init; } = string.Empty;
    public Guid? ParentId { get; init; }
    public bool IsEditMode { get; init; }
    public IReadOnlyList<CategoryLookupItemVm> ParentOptions { get; init; } = Array.Empty<CategoryLookupItemVm>();
}
