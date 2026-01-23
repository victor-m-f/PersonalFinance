using PersonalFinance.Domain.Entities;
using PersonalFinance.Domain.ValueObjects;

namespace PersonalFinance.Tests;

public sealed class CategoryTests
{
    [Fact]
    public void Create_WithValidData_GeneratesIdAndKeepsUpdatedAtNull()
    {
        // Arrange
        var color = CategoryColor.Create("#FF112233");

        // Act
        var result = Category.Create("Food", color.Value!, null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value!.Id);
        Assert.Null(result.Value!.UpdatedAt);
    }

    [Fact]
    public void Rename_WhenNameChanges_UpdatesUpdatedAt()
    {
        // Arrange
        var color = CategoryColor.Create("#FF112233");
        var result = Category.Create("Food", color.Value!, null);
        var category = result.Value!;

        // Act
        var noChange = category.Rename("Food");

        // Assert
        Assert.True(noChange.IsSuccess);
        Assert.Null(category.UpdatedAt);

        // Act
        var changed = category.Rename("Groceries");

        // Assert
        Assert.True(changed.IsSuccess);
        Assert.NotNull(category.UpdatedAt);
    }
}
