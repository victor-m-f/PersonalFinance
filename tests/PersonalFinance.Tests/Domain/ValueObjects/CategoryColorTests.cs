using PersonalFinance.Domain.ValueObjects;

namespace PersonalFinance.Tests.Domain.ValueObjects;

public sealed class CategoryColorTests
{
    [Fact]
    public void Create_WithLowercaseColor_NormalizesToUppercase()
    {
        // Arrange
        var colorHex = "#a1b2c3d4";

        // Act
        var result = CategoryColor.Create(colorHex);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("#A1B2C3D4", result.Value!.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("#123")]
    [InlineData("#1234567")]
    [InlineData("#123456789")]
    [InlineData("12345678")]
    [InlineData("#ZZZZZZZZ")]
    public void Create_WithInvalidValue_ReturnsFailure(string colorHex)
    {
        // Arrange

        // Act
        var result = CategoryColor.Create(colorHex);

        // Assert
        Assert.False(result.IsSuccess);
    }
}
