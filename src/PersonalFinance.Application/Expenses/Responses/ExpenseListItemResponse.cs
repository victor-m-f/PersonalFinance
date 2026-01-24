namespace PersonalFinance.Application.Expenses.Responses;

public sealed record class ExpenseListItemResponse
{
    public Guid Id { get; init; }
    public DateTime Date { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public string? CategoryColorHex { get; init; }
}
