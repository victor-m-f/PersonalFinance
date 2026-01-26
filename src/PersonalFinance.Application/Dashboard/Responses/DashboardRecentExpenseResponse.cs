namespace PersonalFinance.Application.Dashboard.Responses;

public sealed record class DashboardRecentExpenseResponse
{
    public Guid Id { get; init; }
    public DateTime Date { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public string? CategoryColorHex { get; init; }
}
