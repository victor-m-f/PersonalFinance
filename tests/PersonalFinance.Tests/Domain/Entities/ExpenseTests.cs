using PersonalFinance.Domain.Entities;

namespace PersonalFinance.Tests.Domain.Entities;

public sealed class ExpenseTests
{
    [Fact]
    public void Create_ComDadosValidos_GeraIdEUpdatedAtNulo()
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
    public void Create_ComDataInvalida_RetornaFalha()
    {
        // Arrange

        // Act
        var result = Expense.Create(default, 25m, "Lunch", null);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ComValorInvalido_RetornaFalha(decimal amount)
    {
        // Arrange
        var date = new DateTime(2024, 1, 1);

        // Act
        var result = Expense.Create(date, amount, "Lunch", null);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Update_ComDadosIguais_NaoAtualizaUpdatedAt()
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
    }

    [Fact]
    public void Update_ComDadosDiferentes_AtualizaUpdatedAt()
    {
        // Arrange
        var date = new DateTime(2024, 1, 1);
        var result = Expense.Create(date, 25m, "Lunch", null);
        var expense = result.Value!;

        // Act
        var changed = expense.Update(date.AddDays(1), 30m, "Dinner");

        // Assert
        Assert.True(changed.IsSuccess);
        Assert.NotNull(expense.UpdatedAt);
    }

    [Fact]
    public void Update_ComDataInvalida_RetornaFalha()
    {
        // Arrange
        var date = new DateTime(2024, 1, 1);
        var result = Expense.Create(date, 25m, "Lunch", null);
        var expense = result.Value!;

        // Act
        var updateResult = expense.Update(default, 25m, "Lunch");

        // Assert
        Assert.False(updateResult.IsSuccess);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Update_ComValorInvalido_RetornaFalha(decimal amount)
    {
        // Arrange
        var date = new DateTime(2024, 1, 1);
        var result = Expense.Create(date, 25m, "Lunch", null);
        var expense = result.Value!;

        // Act
        var updateResult = expense.Update(date, amount, "Lunch");

        // Assert
        Assert.False(updateResult.IsSuccess);
    }

    [Fact]
    public void AssignCategory_ComMesmoId_NaoAtualizaUpdatedAt()
    {
        // Arrange
        var date = new DateTime(2024, 1, 1);
        var categoryId = Guid.NewGuid();
        var result = Expense.Create(date, 25m, "Lunch", categoryId);
        var expense = result.Value!;

        // Act
        var assignResult = expense.AssignCategory(categoryId);

        // Assert
        Assert.True(assignResult.IsSuccess);
        Assert.Null(expense.UpdatedAt);
    }

    [Fact]
    public void AssignCategory_ComIdDiferente_AtualizaUpdatedAt()
    {
        // Arrange
        var date = new DateTime(2024, 1, 1);
        var categoryId = Guid.NewGuid();
        var result = Expense.Create(date, 25m, "Lunch", categoryId);
        var expense = result.Value!;
        var newCategoryId = Guid.NewGuid();

        // Act
        var assignResult = expense.AssignCategory(newCategoryId);

        // Assert
        Assert.True(assignResult.IsSuccess);
        Assert.NotNull(expense.UpdatedAt);
    }
}
