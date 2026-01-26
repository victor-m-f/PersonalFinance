using PersonalFinance.Application.Dashboard.Responses;

namespace PersonalFinance.Application.Dashboard.Abstractions;

public interface IDashboardReadRepository
{
    public Task<DashboardTotalsResponse> GetTotalsAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct);

    public Task<IReadOnlyList<DashboardCategoryTotalResponse>> GetCategoryTotalsAsync(
        DateTime startDate,
        DateTime endDate,
        int take,
        CancellationToken ct);

    public Task<IReadOnlyList<DashboardRecentExpenseResponse>> GetRecentExpensesAsync(
        DateTime startDate,
        DateTime endDate,
        int take,
        CancellationToken ct);
}
