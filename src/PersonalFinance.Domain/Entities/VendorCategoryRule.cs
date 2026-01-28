using PersonalFinance.Domain.ValueObjects;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Domain.Entities;

public sealed class VendorCategoryRule : EntityBase
{
    public string Keyword { get; private set; } = string.Empty;
    public string KeywordNormalized { get; private set; } = string.Empty;
    public Guid CategoryId { get; private set; }
    public ConfidenceScore Confidence { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? LastUsedAt { get; private set; }

    private VendorCategoryRule(
        string keyword,
        string keywordNormalized,
        Guid categoryId,
        ConfidenceScore confidence,
        DateTimeOffset createdAt)
    {
        Keyword = keyword;
        KeywordNormalized = keywordNormalized;
        CategoryId = categoryId;
        Confidence = confidence;
        CreatedAt = createdAt;
    }

    private VendorCategoryRule() { }

    public static Result<VendorCategoryRule> Create(
        string keyword,
        string keywordNormalized,
        Guid categoryId,
        ConfidenceScore confidence)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return Result<VendorCategoryRule>.Failure(Errors.ValidationError, "Keyword is required.");
        }

        if (string.IsNullOrWhiteSpace(keywordNormalized))
        {
            return Result<VendorCategoryRule>.Failure(Errors.ValidationError, "Keyword normalization is required.");
        }

        if (categoryId == Guid.Empty)
        {
            return Result<VendorCategoryRule>.Failure(Errors.ValidationError, "Category is required.");
        }

        if (confidence is null)
        {
            return Result<VendorCategoryRule>.Failure(Errors.ValidationError, "Confidence is required.");
        }

        var rule = new VendorCategoryRule(
            keyword.Trim(),
            keywordNormalized.Trim(),
            categoryId,
            confidence,
            DateTimeOffset.UtcNow);

        return Result<VendorCategoryRule>.Success(rule);
    }

    public Result UpdateCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
        {
            return Result.Failure(Errors.ValidationError, "Category is required.");
        }

        CategoryId = categoryId;
        return Result.Success();
    }

    public Result UpdateConfidence(ConfidenceScore confidence)
    {
        if (confidence is null)
        {
            return Result.Failure(Errors.ValidationError, "Confidence is required.");
        }

        Confidence = confidence;
        return Result.Success();
    }

    public Result MarkUsed()
    {
        LastUsedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
