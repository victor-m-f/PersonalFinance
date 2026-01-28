using PersonalFinance.Domain.Entities;
using PersonalFinance.Domain.ValueObjects;

namespace PersonalFinance.Tests.Domain.Entities;

public sealed class ExpenseDraftItemTests
{
    private static ConfidenceScore CreateConfidence()
    {
        return ConfidenceScore.Create(0.9).Value!;
    }

    [Fact]
    public void Create_ComDadosValidos_CriaItem()
    {
        // Arrange
        var confidence = CreateConfidence();
        var date = new DateTime(2024, 1, 1);

        // Act
        var result = ExpenseDraftItem.Create(date, 10m, "Item", null, "Food", confidence);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(date, result.Value!.Date);
        Assert.Equal(10m, result.Value!.Amount);
        Assert.Equal("Item", result.Value!.Description);
        Assert.Equal("Food", result.Value!.CategoryName);
        Assert.Equal(confidence, result.Value!.Confidence);
    }

    [Fact]
    public void Create_ComDataInvalida_RetornaFalha()
    {
        // Arrange
        var confidence = CreateConfidence();

        // Act
        var result = ExpenseDraftItem.Create(default, 10m, "Item", null, "Food", confidence);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Create_ComValorInvalido_RetornaFalha(decimal amount)
    {
        // Arrange
        var confidence = CreateConfidence();
        var date = new DateTime(2024, 1, 1);

        // Act
        var result = ExpenseDraftItem.Create(date, amount, "Item", null, "Food", confidence);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ComConfiancaNula_RetornaFalha()
    {
        // Arrange
        var date = new DateTime(2024, 1, 1);

        // Act
        var result = ExpenseDraftItem.Create(date, 10m, "Item", null, "Food", null!);

        // Assert
        Assert.False(result.IsSuccess);
    }
}
