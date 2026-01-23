namespace PersonalFinance.Application.Categories.Requests;

public sealed record class DeleteCategoryRequest
{
    public Guid Id { get; init; }
}
