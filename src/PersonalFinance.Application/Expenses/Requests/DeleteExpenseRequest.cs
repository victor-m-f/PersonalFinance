namespace PersonalFinance.Application.Expenses.Requests;

public sealed record class DeleteExpenseRequest
{
    public Guid Id { get; init; }
}
