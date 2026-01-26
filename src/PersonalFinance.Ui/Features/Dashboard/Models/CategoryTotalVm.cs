namespace PersonalFinance.Ui.Features.Dashboard.Models;

public sealed record class CategoryTotalVm
{
    public string Name { get; init; } = string.Empty;
    public string? ColorHex { get; init; }
    public decimal TotalSpent { get; init; }
}
