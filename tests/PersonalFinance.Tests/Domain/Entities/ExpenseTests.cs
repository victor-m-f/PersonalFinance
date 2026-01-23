using PersonalFinance.Domain.Entities;

namespace PersonalFinance.Tests;

public sealed class ExpenseTests
{
    [Fact]
    public void Create_WithValidData_GeneratesIdAndKeepsUpdatedAtNull()
    {
        // Arrange
        var date = new DateTime(2024, 1, 1);

        // Act
        var result = Expense.Create(date, 25m, "Lunch", null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value!.Id);
        Assert.Null(result.Value!.UpdatedAt);
    }

    [Fact]
    public void Update_WhenNoChanges_DoesNotUpdateUpdatedAt()
    {
        // Arrange
        var date = new DateTime(2024, 1, 1);
        var result = Expense.Create(date, 25m, "Lunch", null);
        var expense = result.Value!;

        // Act
        var noChange = expense.Update(date, 25m, "Lunch");

        // Assert
        Assert.True(noChange.IsSuccess);
        Assert.Null(expense.UpdatedAt);

        // Act
        var changed = expense.Update(date.AddDays(1), 30m, "Dinner");

        // Assert
        Assert.True(changed.IsSuccess);
        Assert.NotNull(expense.UpdatedAt);
    }
}
