using PersonalFinance.Shared.Results;

namespace PersonalFinance.Domain.Entities;

public sealed class Expense
{
    public Guid Id { get; private set; }
    public DateTime Date { get; private set; }
    public decimal Amount { get; private set; }
    public string? Description { get; private set; }
    public Guid? CategoryId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Expense(Guid id, DateTime date, decimal amount, string? description, Guid? categoryId, DateTimeOffset createdAt, DateTimeOffset updatedAt)
    {
        Id = id;
        Date = date;
        Amount = amount;
        Description = description;
        CategoryId = categoryId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Result<Expense> Create(Guid id, DateTime date, decimal amount, string? description, Guid? categoryId)
    {
        var validation = ValidateDetails(date, amount, description);
        if (!validation.IsSuccess)
        {
            return Result<Expense>.Failure(validation.ErrorCode ?? Errors.ValidationError, validation.ErrorMessage ?? "Invalid expense.");
        }

        var now = DateTimeOffset.UtcNow;
        var expense = new Expense(id, date, amount, validation.Value, categoryId, now, now);
        return Result<Expense>.Success(expense);
    }

    public Result Update(DateTime date, decimal amount, string? description)
    {
        var validation = ValidateDetails(date, amount, description);
        if (!validation.IsSuccess)
        {
            return Result.Failure(validation.ErrorCode ?? Errors.ValidationError, validation.ErrorMessage ?? "Invalid expense.");
        }

        Date = date;
        Amount = amount;
        Description = validation.Value;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result AssignCategory(Guid? categoryId)
    {
        CategoryId = categoryId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    private static Result<string?> ValidateDetails(DateTime date, decimal amount, string? description)
    {
        if (date == default)
        {
            return Result<string?>.Failure(Errors.ValidationError, "Date is required.");
        }

        if (amount <= 0)
        {
            return Result<string?>.Failure(Errors.ValidationError, "Amount must be greater than zero.");
        }

        if (description is null)
        {
            return Result<string?>.Success(null);
        }

        var trimmed = description.Trim();
        return Result<string?>.Success(trimmed);
    }
}
