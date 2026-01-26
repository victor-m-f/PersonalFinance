namespace PersonalFinance.Application.Dashboard.Requests;

public sealed record class DashboardPeriodRequest
{
    public int? Year { get; init; }
    public int? Month { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
