using PersonalFinance.Shared.Results;

namespace PersonalFinance.Domain.Entities;

public sealed class Expense : EntityBase
{
    public DateTime Date { get; private set; }
    public decimal Amount { get; private set; }
    public string? Description { get; private set; }
    public string? DescriptionSearch { get; private set; }
    public Guid? CategoryId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    private Expense(
        DateTime date,
        decimal amount,
        string? description,
        Guid? categoryId,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt)
    {
        Date = date;
        Amount = amount;
        Description = description;
        CategoryId = categoryId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    private Expense() { }

    public static Result<Expense> Create(
        DateTime date,
        decimal amount,
        string? description,
        Guid? categoryId)
    {
        var validation = ValidateDetails(date, amount);
        if (!validation.IsSuccess)
        {
            return Result<Expense>.Failure(
                validation.ErrorCode ?? Errors.ValidationError,
                validation.ErrorMessage ?? "Invalid expense.");
        }

        var now = DateTimeOffset.UtcNow;
        var expense = new Expense(date, amount, description, categoryId, now, null);
        return Result<Expense>.Success(expense);
    }

    public Result Update(DateTime date, decimal amount, string? description)
    {
        var validation = ValidateDetails(date, amount);
        if (!validation.IsSuccess)
        {
            return Result.Failure(
                validation.ErrorCode ?? Errors.ValidationError,
                validation.ErrorMessage ?? "Invalid expense.");
        }

        if (Date == date && Amount == amount && string.Equals(Description, description, StringComparison.Ordinal))
        {
            return Result.Success();
        }

        Date = date;
        Amount = amount;
        Description = description;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result AssignCategory(Guid? categoryId)
    {
        if (CategoryId == categoryId)
        {
            return Result.Success();
        }

        CategoryId = categoryId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    private static Result ValidateDetails(DateTime date, decimal amount)
    {
        if (date == default)
        {
            return Result.Failure(Errors.ValidationError, "Date is required.");
        }

        if (amount <= 0)
        {
            return Result.Failure(Errors.ValidationError, "Amount must be greater than zero.");
        }

        return Result.Success();
    }
}
