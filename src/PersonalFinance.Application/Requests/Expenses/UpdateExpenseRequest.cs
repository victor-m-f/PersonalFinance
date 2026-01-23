namespace PersonalFinance.Application.Requests.Expenses;

public sealed record class UpdateExpenseRequest
{
    public Guid Id { get; init; }
    public DateTime Date { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
}
