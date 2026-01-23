using PersonalFinance.Domain.ValueObjects;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Domain.Entities;

public sealed class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string ColorHex { get; private set; }
    public Guid? ParentId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Category(Guid id, string name, string colorHex, Guid? parentId, DateTimeOffset createdAt, DateTimeOffset updatedAt)
    {
        Id = id;
        Name = name;
        ColorHex = colorHex;
        ParentId = parentId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Result<Category> Create(Guid id, string name, string colorHex, Guid? parentId)
    {
        var nameResult = ValidateName(name);
        if (!nameResult.IsSuccess)
        {
            return Result<Category>.Failure(nameResult.ErrorCode ?? Errors.ValidationError, nameResult.ErrorMessage ?? "Invalid name.");
        }

        var colorResult = CategoryColor.Create(colorHex);
        if (!colorResult.IsSuccess)
        {
            return Result<Category>.Failure(colorResult.ErrorCode ?? Errors.ValidationError, colorResult.ErrorMessage ?? "Invalid color.");
        }

        if (parentId.HasValue && parentId.Value == id)
        {
            return Result<Category>.Failure(Errors.ValidationError, "Category cannot be its own parent.");
        }

        var now = DateTimeOffset.UtcNow;
        var category = new Category(id, nameResult.Value!, colorResult.Value!.Value, parentId, now, now);
        return Result<Category>.Success(category);
    }

    public Result Rename(string name)
    {
        var nameResult = ValidateName(name);
        if (!nameResult.IsSuccess)
        {
            return Result.Failure(nameResult.ErrorCode ?? Errors.ValidationError, nameResult.ErrorMessage ?? "Invalid name.");
        }

        Name = nameResult.Value!;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result SetColor(string colorHex)
    {
        var colorResult = CategoryColor.Create(colorHex);
        if (!colorResult.IsSuccess)
        {
            return Result.Failure(colorResult.ErrorCode ?? Errors.ValidationError, colorResult.ErrorMessage ?? "Invalid color.");
        }

        ColorHex = colorResult.Value!.Value;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result SetParent(Guid? parentId)
    {
        if (parentId.HasValue && parentId.Value == Id)
        {
            return Result.Failure(Errors.ValidationError, "Category cannot be its own parent.");
        }

        ParentId = parentId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    private static Result<string> ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<string>.Failure(Errors.ValidationError, "Name is required.");
        }

        var trimmed = name.Trim();

        if (trimmed.Length < 2 || trimmed.Length > 60)
        {
            return Result<string>.Failure(Errors.ValidationError, "Name must be between 2 and 60 characters.");
        }

        return Result<string>.Success(trimmed);
    }
}
