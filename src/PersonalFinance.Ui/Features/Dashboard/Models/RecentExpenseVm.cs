namespace PersonalFinance.Ui.Features.Dashboard.Models;

public sealed record class RecentExpenseVm
{
    public string DateText { get; init; } = string.Empty;
    public string DescriptionText { get; init; } = string.Empty;
    public string CategoryText { get; init; } = string.Empty;
    public string AmountText { get; init; } = string.Empty;
    public string? CategoryColorHex { get; init; }
}
