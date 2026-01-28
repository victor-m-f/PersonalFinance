using PersonalFinance.Shared.Results;

namespace PersonalFinance.Domain.ValueObjects;

public sealed record class DocumentHash
{
    public string Value { get; } = string.Empty;

    private DocumentHash(string value)
    {
        Value = value;
    }

    private DocumentHash() { }

    public static Result<DocumentHash> Create(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            return Result<DocumentHash>.Failure(Errors.ValidationError, "Hash is required.");
        }

        var normalized = hash.Trim().ToLowerInvariant();
        if (normalized.Length != 64 || !normalized.All(Uri.IsHexDigit))
        {
            return Result<DocumentHash>.Failure(Errors.ValidationError, "Hash must be a valid SHA-256 hex string.");
        }

        return Result<DocumentHash>.Success(new DocumentHash(normalized));
    }

    public static DocumentHash FromStorage(string hash)
    {
        return new DocumentHash(hash);
    }
}
