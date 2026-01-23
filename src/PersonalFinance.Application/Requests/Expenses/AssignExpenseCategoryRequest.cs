namespace PersonalFinance.Application.Requests.Expenses;

public sealed record class AssignExpenseCategoryRequest
{
    public Guid Id { get; init; }
    public Guid? CategoryId { get; init; }
}
