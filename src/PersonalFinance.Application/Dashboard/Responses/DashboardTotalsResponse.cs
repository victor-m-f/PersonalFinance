namespace PersonalFinance.Application.Dashboard.Responses;

public sealed record class DashboardTotalsResponse
{
    public decimal TotalSpent { get; init; }
    public int ExpensesCount { get; init; }
}
