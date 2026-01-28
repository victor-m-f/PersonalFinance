using PersonalFinance.Domain.ValueObjects;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Domain.Entities;

public sealed class ExpenseDraftItem
{
    public DateTime Date { get; private set; }
    public decimal Amount { get; private set; }
    public string? Description { get; private set; }
    public Guid? CategoryId { get; private set; }
    public string? CategoryName { get; private set; }
    public ConfidenceScore Confidence { get; private set; } = default!;

    private ExpenseDraftItem(
        DateTime date,
        decimal amount,
        string? description,
        Guid? categoryId,
        string? categoryName,
        ConfidenceScore confidence)
    {
        Date = date;
        Amount = amount;
        Description = description;
        CategoryId = categoryId;
        CategoryName = categoryName;
        Confidence = confidence;
    }

    private ExpenseDraftItem() { }

    public static Result<ExpenseDraftItem> Create(
        DateTime date,
        decimal amount,
        string? description,
        Guid? categoryId,
        string? categoryName,
        ConfidenceScore confidence)
    {
        if (date == default)
        {
            return Result<ExpenseDraftItem>.Failure(Errors.ValidationError, "Date is required.");
        }

        if (amount <= 0)
        {
            return Result<ExpenseDraftItem>.Failure(Errors.ValidationError, "Amount must be greater than zero.");
        }

        if (confidence is null)
        {
            return Result<ExpenseDraftItem>.Failure(Errors.ValidationError, "Confidence is required.");
        }

        var item = new ExpenseDraftItem(date, amount, description, categoryId, categoryName, confidence);
        return Result<ExpenseDraftItem>.Success(item);
    }
}
