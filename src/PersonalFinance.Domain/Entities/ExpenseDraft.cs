using PersonalFinance.Domain.ValueObjects;
using PersonalFinance.Shared.Results;

namespace PersonalFinance.Domain.Entities;

public sealed class ExpenseDraft : EntityBase
{
    private readonly List<ExpenseDraftItem> _items = new();

    public Guid DocumentId { get; private set; }
    public string RawText { get; private set; } = string.Empty;
    public bool IsOcrUsed { get; private set; }
    public ConfidenceScore OverallConfidence { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyList<ExpenseDraftItem> Items => _items;

    private ExpenseDraft(
        Guid documentId,
        string rawText,
        bool isOcrUsed,
        ConfidenceScore overallConfidence,
        DateTimeOffset createdAt,
        IEnumerable<ExpenseDraftItem> items)
    {
        DocumentId = documentId;
        RawText = rawText;
        IsOcrUsed = isOcrUsed;
        OverallConfidence = overallConfidence;
        CreatedAt = createdAt;
        _items.AddRange(items);
    }

    private ExpenseDraft() { }

    public static Result<ExpenseDraft> Create(
        Guid documentId,
        string rawText,
        bool isOcrUsed,
        ConfidenceScore overallConfidence,
        IEnumerable<ExpenseDraftItem> items)
    {
        if (documentId == Guid.Empty)
        {
            return Result<ExpenseDraft>.Failure(Errors.ValidationError, "Document id is required.");
        }

        if (overallConfidence is null)
        {
            return Result<ExpenseDraft>.Failure(Errors.ValidationError, "Confidence is required.");
        }

        var draft = new ExpenseDraft(
            documentId,
            rawText ?? string.Empty,
            isOcrUsed,
            overallConfidence,
            DateTimeOffset.UtcNow,
            items ?? Array.Empty<ExpenseDraftItem>());

        return Result<ExpenseDraft>.Success(draft);
    }
}
