using PersonalFinance.Domain.Entities;
using PersonalFinance.Domain.ValueObjects;

namespace PersonalFinance.Tests.Domain.Entities;

public sealed class VendorCategoryRuleTests
{
    private static ConfidenceScore CreateConfidence()
    {
        return ConfidenceScore.Create(0.6).Value!;
    }

    [Fact]
    public void Create_ComDadosValidos_NormalizaCampos()
    {
        // Arrange
        var confidence = CreateConfidence();
        var categoryId = Guid.NewGuid();

        // Act
        var result = VendorCategoryRule.Create(" Amazon ", " amazon ", categoryId, confidence);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Amazon", result.Value!.Keyword);
        Assert.Equal("amazon", result.Value!.KeywordNormalized);
        Assert.Equal(categoryId, result.Value!.CategoryId);
        Assert.Equal(confidence, result.Value!.Confidence);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_ComKeywordVazio_RetornaFalha(string keyword)
    {
        // Arrange
        var confidence = CreateConfidence();
        var categoryId = Guid.NewGuid();

        // Act
        var result = VendorCategoryRule.Create(keyword, "amazon", categoryId, confidence);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_ComKeywordNormalizedVazio_RetornaFalha(string keywordNormalized)
    {
        // Arrange
        var confidence = CreateConfidence();
        var categoryId = Guid.NewGuid();

        // Act
        var result = VendorCategoryRule.Create("Amazon", keywordNormalized, categoryId, confidence);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ComCategoryIdVazio_RetornaFalha()
    {
        // Arrange
        var confidence = CreateConfidence();

        // Act
        var result = VendorCategoryRule.Create("Amazon", "amazon", Guid.Empty, confidence);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ComConfiancaNula_RetornaFalha()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        // Act
        var result = VendorCategoryRule.Create("Amazon", "amazon", categoryId, null!);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void UpdateCategory_ComIdVazio_RetornaFalha()
    {
        // Arrange
        var confidence = CreateConfidence();
        var categoryId = Guid.NewGuid();
        var rule = VendorCategoryRule.Create("Amazon", "amazon", categoryId, confidence).Value!;

        // Act
        var updateResult = rule.UpdateCategory(Guid.Empty);

        // Assert
        Assert.False(updateResult.IsSuccess);
    }

    [Fact]
    public void UpdateCategory_ComIdValido_AtualizaCategoria()
    {
        // Arrange
        var confidence = CreateConfidence();
        var categoryId = Guid.NewGuid();
        var rule = VendorCategoryRule.Create("Amazon", "amazon", categoryId, confidence).Value!;
        var newCategoryId = Guid.NewGuid();

        // Act
        var updateResult = rule.UpdateCategory(newCategoryId);

        // Assert
        Assert.True(updateResult.IsSuccess);
        Assert.Equal(newCategoryId, rule.CategoryId);
    }

    [Fact]
    public void UpdateConfidence_ComNula_RetornaFalha()
    {
        // Arrange
        var confidence = CreateConfidence();
        var categoryId = Guid.NewGuid();
        var rule = VendorCategoryRule.Create("Amazon", "amazon", categoryId, confidence).Value!;

        // Act
        var updateResult = rule.UpdateConfidence(null!);

        // Assert
        Assert.False(updateResult.IsSuccess);
    }

    [Fact]
    public void UpdateConfidence_ComValida_AtualizaConfianca()
    {
        // Arrange
        var confidence = CreateConfidence();
        var categoryId = Guid.NewGuid();
        var rule = VendorCategoryRule.Create("Amazon", "amazon", categoryId, confidence).Value!;
        var newConfidence = ConfidenceScore.Create(0.95).Value!;

        // Act
        var updateResult = rule.UpdateConfidence(newConfidence);

        // Assert
        Assert.True(updateResult.IsSuccess);
        Assert.Equal(newConfidence, rule.Confidence);
    }

    [Fact]
    public void MarkUsed_ComChamada_AtualizaLastUsedAt()
    {
        // Arrange
        var confidence = CreateConfidence();
        var categoryId = Guid.NewGuid();
        var rule = VendorCategoryRule.Create("Amazon", "amazon", categoryId, confidence).Value!;

        // Act
        var used = rule.MarkUsed();

        // Assert
        Assert.True(used.IsSuccess);
        Assert.NotNull(rule.LastUsedAt);
    }
}
