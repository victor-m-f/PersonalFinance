namespace PersonalFinance.Application.Categories.Responses;

public sealed record class CategoryIdResponse
{
    public required Guid Id { get; init; }
}
