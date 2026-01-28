using PersonalFinance.Domain.Entities;
using PersonalFinance.Domain.ValueObjects;

namespace PersonalFinance.Tests.Domain.Entities;

public sealed class ImportedDocumentTests
{
    private static DocumentHash CreateHash()
    {
        var hashValue = new string('a', 64);
        return DocumentHash.Create(hashValue).Value!;
    }

    [Fact]
    public void Create_ComDadosValidos_NormalizaCamposEStatusUploaded()
    {
        // Arrange
        var hash = CreateHash();

        // Act
        var result = ImportedDocument.Create(" invoice.pdf ", " stored ", " PDF ", 123, hash);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("invoice.pdf", result.Value!.OriginalFileName);
        Assert.Equal("stored", result.Value!.StoredFileName);
        Assert.Equal("pdf", result.Value!.FileExtension);
        Assert.Equal(ImportedDocumentStatus.Uploaded, result.Value!.Status);
        Assert.False(result.Value!.IsOcrUsed);
        Assert.Null(result.Value!.ProcessedAt);
        Assert.Null(result.Value!.FailureReason);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_ComNomeOriginalVazio_RetornaFalha(string name)
    {
        // Arrange
        var hash = CreateHash();

        // Act
        var result = ImportedDocument.Create(name, "stored", "pdf", 123, hash);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_ComNomeArmazenadoVazio_RetornaFalha(string name)
    {
        // Arrange
        var hash = CreateHash();

        // Act
        var result = ImportedDocument.Create("invoice.pdf", name, "pdf", 123, hash);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_ComExtensaoVazia_RetornaFalha(string extension)
    {
        // Arrange
        var hash = CreateHash();

        // Act
        var result = ImportedDocument.Create("invoice.pdf", "stored", extension, 123, hash);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ComHashNulo_RetornaFalha()
    {
        // Arrange

        // Act
        var result = ImportedDocument.Create("invoice.pdf", "stored", "pdf", 123, null!);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void MarkParsed_ComDocumentoNaoConfirmado_DefineStatusParsedEProcessado()
    {
        // Arrange
        var hash = CreateHash();
        var result = ImportedDocument.Create("invoice.pdf", "stored", "pdf", 123, hash);
        var document = result.Value!;

        // Act
        var parsed = document.MarkParsed(true);

        // Assert
        Assert.True(parsed.IsSuccess);
        Assert.Equal(ImportedDocumentStatus.Parsed, document.Status);
        Assert.True(document.IsOcrUsed);
        Assert.NotNull(document.ProcessedAt);
        Assert.Null(document.FailureReason);
    }

    [Fact]
    public void MarkParsed_ComDocumentoConfirmado_RetornaFalha()
    {
        // Arrange
        var hash = CreateHash();
        var result = ImportedDocument.Create("invoice.pdf", "stored", "pdf", 123, hash);
        var document = result.Value!;
        document.MarkConfirmed();

        // Act
        var parsed = document.MarkParsed(false);

        // Assert
        Assert.False(parsed.IsSuccess);
        Assert.Equal(ImportedDocumentStatus.Confirmed, document.Status);
    }

    [Fact]
    public void MarkConfirmed_ComDocumentoNaoConfirmado_DefineStatusConfirmedEProcessado()
    {
        // Arrange
        var hash = CreateHash();
        var result = ImportedDocument.Create("invoice.pdf", "stored", "pdf", 123, hash);
        var document = result.Value!;

        // Act
        var confirmed = document.MarkConfirmed();

        // Assert
        Assert.True(confirmed.IsSuccess);
        Assert.Equal(ImportedDocumentStatus.Confirmed, document.Status);
        Assert.NotNull(document.ProcessedAt);
        Assert.Null(document.FailureReason);
    }

    [Fact]
    public void MarkConfirmed_ComDocumentoJaConfirmado_MantemStatusConfirmed()
    {
        // Arrange
        var hash = CreateHash();
        var result = ImportedDocument.Create("invoice.pdf", "stored", "pdf", 123, hash);
        var document = result.Value!;
        document.MarkConfirmed();

        // Act
        var confirmed = document.MarkConfirmed();

        // Assert
        Assert.True(confirmed.IsSuccess);
        Assert.Equal(ImportedDocumentStatus.Confirmed, document.Status);
    }

    [Fact]
    public void MarkFailed_ComMotivoVazio_DefineUnknown()
    {
        // Arrange
        var hash = CreateHash();
        var result = ImportedDocument.Create("invoice.pdf", "stored", "pdf", 123, hash);
        var document = result.Value!;

        // Act
        var failed = document.MarkFailed(" ");

        // Assert
        Assert.True(failed.IsSuccess);
        Assert.Equal(ImportedDocumentStatus.Failed, document.Status);
        Assert.Equal("Unknown", document.FailureReason);
        Assert.NotNull(document.ProcessedAt);
    }

    [Fact]
    public void MarkFailed_ComMotivo_DefineMotivoEStatusFailed()
    {
        // Arrange
        var hash = CreateHash();
        var result = ImportedDocument.Create("invoice.pdf", "stored", "pdf", 123, hash);
        var document = result.Value!;

        // Act
        var failed = document.MarkFailed("Invalid format");

        // Assert
        Assert.True(failed.IsSuccess);
        Assert.Equal(ImportedDocumentStatus.Failed, document.Status);
        Assert.Equal("Invalid format", document.FailureReason);
        Assert.NotNull(document.ProcessedAt);
    }
}
