namespace PersonalFinance.Ui.Features.Dashboard.Models;

public sealed record class MonthOptionVm
{
    public int Value { get; init; }
    public string Name { get; init; } = string.Empty;
}
