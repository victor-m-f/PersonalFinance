using PersonalFinance.Domain.ValueObjects;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Domain.Entities;

public sealed class ImportedDocument : EntityBase
{
    public string OriginalFileName { get; private set; } = string.Empty;
    public string StoredFileName { get; private set; } = string.Empty;
    public string FileExtension { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public DocumentHash Hash { get; private set; } = default!;
    public ImportedDocumentStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public bool IsOcrUsed { get; private set; }
    public string? FailureReason { get; private set; }

    private ImportedDocument(
        string originalFileName,
        string storedFileName,
        string fileExtension,
        long fileSize,
        DocumentHash hash,
        DateTimeOffset createdAt)
    {
        OriginalFileName = originalFileName;
        StoredFileName = storedFileName;
        FileExtension = fileExtension;
        FileSize = fileSize;
        Hash = hash;
        Status = ImportedDocumentStatus.Uploaded;
        CreatedAt = createdAt;
        IsOcrUsed = false;
    }

    private ImportedDocument() { }

    public static Result<ImportedDocument> Create(
        string originalFileName,
        string storedFileName,
        string fileExtension,
        long fileSize,
        DocumentHash hash)
    {
        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            return Result<ImportedDocument>.Failure(Errors.ValidationError, "Original file name is required.");
        }

        if (string.IsNullOrWhiteSpace(storedFileName))
        {
            return Result<ImportedDocument>.Failure(Errors.ValidationError, "Stored file name is required.");
        }

        if (string.IsNullOrWhiteSpace(fileExtension))
        {
            return Result<ImportedDocument>.Failure(Errors.ValidationError, "File extension is required.");
        }

        if (hash is null)
        {
            return Result<ImportedDocument>.Failure(Errors.ValidationError, "Hash is required.");
        }

        var now = DateTimeOffset.UtcNow;
        var document = new ImportedDocument(
            originalFileName.Trim(),
            storedFileName.Trim(),
            fileExtension.Trim().ToLowerInvariant(),
            fileSize,
            hash,
            now);

        return Result<ImportedDocument>.Success(document);
    }

    public Result MarkParsed(bool isOcrUsed)
    {
        if (Status == ImportedDocumentStatus.Confirmed)
        {
            return Result.Failure(Errors.Conflict, "Document already confirmed.");
        }

        Status = ImportedDocumentStatus.Parsed;
        IsOcrUsed = isOcrUsed;
        ProcessedAt = DateTimeOffset.UtcNow;
        FailureReason = null;
        return Result.Success();
    }

    public Result MarkConfirmed()
    {
        if (Status == ImportedDocumentStatus.Confirmed)
        {
            return Result.Success();
        }

        Status = ImportedDocumentStatus.Confirmed;
        ProcessedAt = DateTimeOffset.UtcNow;
        FailureReason = null;
        return Result.Success();
    }

    public Result MarkFailed(string reason)
    {
        Status = ImportedDocumentStatus.Failed;
        FailureReason = string.IsNullOrWhiteSpace(reason) ? "Unknown" : reason.Trim();
        ProcessedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
