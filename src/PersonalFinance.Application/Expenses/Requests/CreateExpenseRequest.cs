namespace PersonalFinance.Application.Expenses.Requests;

public sealed record class CreateExpenseRequest
{
    public DateTime Date { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
}
