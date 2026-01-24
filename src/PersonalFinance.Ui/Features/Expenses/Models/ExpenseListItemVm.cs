namespace PersonalFinance.Ui.Features.Expenses.Models;

public sealed record class ExpenseListItemVm
{
    public Guid Id { get; init; }
    public DateTime Date { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public string? CategoryColorHex { get; init; }
    public string DateDisplay { get; init; } = string.Empty;
    public string AmountDisplay { get; init; } = string.Empty;
    public string DescriptionDisplay { get; init; } = string.Empty;
    public string CategoryDisplay { get; init; } = string.Empty;
}
