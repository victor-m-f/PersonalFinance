namespace PersonalFinance.Application.Expenses.Responses;

public sealed record class ExpenseIdResponse
{
    public Guid Id { get; init; }
}
