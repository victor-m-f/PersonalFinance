namespace PersonalFinance.Ui.Features.Expenses.Models;

public sealed record class ExpenseEditorResult
{
    public Guid? Id { get; init; }
    public DateTime Date { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
    public bool IsEditMode { get; init; }
}
