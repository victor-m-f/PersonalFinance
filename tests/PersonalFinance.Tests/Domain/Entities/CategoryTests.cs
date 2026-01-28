using PersonalFinance.Domain.Entities;
using PersonalFinance.Domain.ValueObjects;

namespace PersonalFinance.Tests.Domain.Entities;

public sealed class CategoryTests
{
    [Fact]
    public void Create_ComDadosValidos_GeraIdEUpdatedAtNulo()
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

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_ComNomeVazio_RetornaFalha(string name)
    {
        // Arrange
        var color = CategoryColor.Create("#FF112233");

        // Act
        var result = Category.Create(name, color.Value!, null);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ComCorNula_RetornaFalha()
    {
        // Arrange

        // Act
        var result = Category.Create("Food", null!, null);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Rename_ComNomeIgual_NaoAtualizaUpdatedAt()
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
    }

    [Fact]
    public void Rename_ComNomeDiferente_AtualizaUpdatedAt()
    {
        // Arrange
        var color = CategoryColor.Create("#FF112233");
        var result = Category.Create("Food", color.Value!, null);
        var category = result.Value!;

        // Act
        var changed = category.Rename("Groceries");

        // Assert
        Assert.True(changed.IsSuccess);
        Assert.NotNull(category.UpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Rename_ComNomeInvalido_RetornaFalha(string name)
    {
        // Arrange
        var color = CategoryColor.Create("#FF112233");
        var result = Category.Create("Food", color.Value!, null);
        var category = result.Value!;

        // Act
        var renameResult = category.Rename(name);

        // Assert
        Assert.False(renameResult.IsSuccess);
    }

    [Fact]
    public void SetColor_ComCorIgual_NaoAtualizaUpdatedAt()
    {
        // Arrange
        var color = CategoryColor.Create("#FF112233");
        var result = Category.Create("Food", color.Value!, null);
        var category = result.Value!;

        // Act
        var setColor = category.SetColor(color.Value!);

        // Assert
        Assert.True(setColor.IsSuccess);
        Assert.Null(category.UpdatedAt);
    }

    [Fact]
    public void SetColor_ComCorDiferente_AtualizaUpdatedAt()
    {
        // Arrange
        var color = CategoryColor.Create("#FF112233");
        var otherColor = CategoryColor.Create("#FFAA2233");
        var result = Category.Create("Food", color.Value!, null);
        var category = result.Value!;

        // Act
        var setColor = category.SetColor(otherColor.Value!);

        // Assert
        Assert.True(setColor.IsSuccess);
        Assert.NotNull(category.UpdatedAt);
    }

    [Fact]
    public void SetColor_ComCorNula_RetornaFalha()
    {
        // Arrange
        var color = CategoryColor.Create("#FF112233");
        var result = Category.Create("Food", color.Value!, null);
        var category = result.Value!;

        // Act
        var setColor = category.SetColor(null!);

        // Assert
        Assert.False(setColor.IsSuccess);
    }

    [Fact]
    public void SetParent_ComMesmoParent_NaoAtualizaUpdatedAt()
    {
        // Arrange
        var color = CategoryColor.Create("#FF112233");
        var parentId = Guid.NewGuid();
        var result = Category.Create("Food", color.Value!, parentId);
        var category = result.Value!;

        // Act
        var setParent = category.SetParent(parentId);

        // Assert
        Assert.True(setParent.IsSuccess);
        Assert.Null(category.UpdatedAt);
    }

    [Fact]
    public void SetParent_ComIdIgual_RetornaFalha()
    {
        // Arrange
        var color = CategoryColor.Create("#FF112233");
        var result = Category.Create("Food", color.Value!, null);
        var category = result.Value!;

        // Act
        var setParent = category.SetParent(category.Id);

        // Assert
        Assert.False(setParent.IsSuccess);
    }

    [Fact]
    public void SetParent_ComParentDiferente_AtualizaUpdatedAt()
    {
        // Arrange
        var color = CategoryColor.Create("#FF112233");
        var result = Category.Create("Food", color.Value!, null);
        var category = result.Value!;
        var parentId = Guid.NewGuid();

        // Act
        var setParent = category.SetParent(parentId);

        // Assert
        Assert.True(setParent.IsSuccess);
        Assert.NotNull(category.UpdatedAt);
    }
}
