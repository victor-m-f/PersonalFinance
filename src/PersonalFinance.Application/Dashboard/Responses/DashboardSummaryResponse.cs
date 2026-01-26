namespace PersonalFinance.Application.Dashboard.Responses;

public sealed record class DashboardSummaryResponse
{
    public decimal TotalSpent { get; init; }
    public decimal DailyAverage { get; init; }
    public int ExpensesCount { get; init; }
    public decimal? PreviousTotalSpent { get; init; }
    public decimal? PeriodChangeAmount { get; init; }
    public decimal? PeriodChangePercent { get; init; }
    public string PeriodChangeText { get; init; } = string.Empty;
}
