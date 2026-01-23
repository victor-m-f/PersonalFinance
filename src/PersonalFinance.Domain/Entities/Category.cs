using PersonalFinance.Domain.ValueObjects;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Domain.Entities;

public sealed class Category : EntityBase
{
    public string Name { get; private set; } = default!;
    public CategoryColor Color { get; private set; } = default!;
    public Guid? ParentId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    private Category(
        string name,
        CategoryColor color,
        Guid? parentId,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt)
    {
        Name = name;
        Color = color;
        ParentId = parentId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    private Category() { }

    public static Result<Category> Create(string name, CategoryColor color, Guid? parentId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<Category>.Failure(Errors.ValidationError, "Name is required.");
        }

        if (color is null)
        {
            return Result<Category>.Failure(Errors.ValidationError, "Color is required.");
        }
        
        var now = DateTimeOffset.UtcNow;
        var category = new Category(name, color, parentId, now, null);

        return Result<Category>.Success(category);
    }

    public Result Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(Errors.ValidationError, "Name is required.");
        }

        if (string.Equals(Name, name, StringComparison.Ordinal))
        {
            return Result.Success();
        }

        Name = name;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result SetColor(CategoryColor color)
    {
        if (color is null)
        {
            return Result.Failure(Errors.ValidationError, "Color is required.");
        }

        if (Equals(Color, color))
        {
            return Result.Success();
        }

        Color = color;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result SetParent(Guid? parentId)
    {
        if (parentId.HasValue && parentId.Value == Id)
        {
            return Result.Failure(Errors.ValidationError, "Category cannot be its own parent.");
        }

        if (ParentId == parentId)
        {
            return Result.Success();
        }

        ParentId = parentId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
