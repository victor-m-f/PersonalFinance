namespace PersonalFinance.Application.Dashboard.Responses;

public sealed record class DashboardCategoryTotalResponse
{
    public Guid? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public string? CategoryColorHex { get; init; }
    public decimal TotalSpent { get; init; }
}
