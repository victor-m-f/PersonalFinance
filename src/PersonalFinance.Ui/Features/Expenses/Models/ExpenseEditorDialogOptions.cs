using PersonalFinance.Ui.Features.Categories.Models;

namespace PersonalFinance.Ui.Features.Expenses.Models;

public sealed record class ExpenseEditorDialogOptions
{
    public Guid? Id { get; init; }
    public DateTime Date { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
    public bool IsEditMode { get; init; }
    public IReadOnlyList<CategoryLookupItemVm> CategoryOptions { get; init; } = Array.Empty<CategoryLookupItemVm>();
}
