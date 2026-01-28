using PersonalFinance.Domain.Entities;
using PersonalFinance.Domain.ValueObjects;

namespace PersonalFinance.Tests.Domain.Entities;

public sealed class ExpenseDraftTests
{
    private static ConfidenceScore CreateConfidence()
    {
        return ConfidenceScore.Create(0.85).Value!;
    }

    private static ExpenseDraftItem CreateItem()
    {
        var confidence = CreateConfidence();
        return ExpenseDraftItem.Create(new DateTime(2024, 1, 1), 10m, "Item", null, "Food", confidence).Value!;
    }

    [Fact]
    public void Create_ComDadosValidos_CriaDraftComItens()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var confidence = CreateConfidence();
        var items = new[] { CreateItem() };

        // Act
        var result = ExpenseDraft.Create(documentId, "raw", true, confidence, items);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value!.Id);
        Assert.Equal(documentId, result.Value!.DocumentId);
        Assert.Equal("raw", result.Value!.RawText);
        Assert.True(result.Value!.IsOcrUsed);
        Assert.Single(result.Value!.Items);
    }

    [Fact]
    public void Create_ComDocumentIdVazio_RetornaFalha()
    {
        // Arrange
        var confidence = CreateConfidence();

        // Act
        var result = ExpenseDraft.Create(Guid.Empty, "raw", false, confidence, Array.Empty<ExpenseDraftItem>());

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ComConfiancaNula_RetornaFalha()
    {
        // Arrange
        var documentId = Guid.NewGuid();

        // Act
        var result = ExpenseDraft.Create(documentId, "raw", false, null!, Array.Empty<ExpenseDraftItem>());

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ComRawTextNulo_DefineStringVazia()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var confidence = CreateConfidence();

        // Act
        var result = ExpenseDraft.Create(documentId, null, false, confidence, Array.Empty<ExpenseDraftItem>());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(string.Empty, result.Value!.RawText);
    }

    [Fact]
    public void Create_ComItensNulos_DefineListaVazia()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var confidence = CreateConfidence();

        // Act
        var result = ExpenseDraft.Create(documentId, "raw", false, confidence, null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!.Items);
    }
}
