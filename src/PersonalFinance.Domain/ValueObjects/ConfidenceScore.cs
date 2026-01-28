using PersonalFinance.Shared.Results;

namespace PersonalFinance.Domain.ValueObjects;

public sealed record class ConfidenceScore
{
    public double Value { get; } = 0d;

    private ConfidenceScore(double value)
    {
        Value = value;
    }

    private ConfidenceScore() { }

    public static Result<ConfidenceScore> Create(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return Result<ConfidenceScore>.Failure(Errors.ValidationError, "Confidence score is invalid.");
        }

        if (value < 0d || value > 1d)
        {
            return Result<ConfidenceScore>.Failure(Errors.ValidationError, "Confidence score must be between 0 and 1.");
        }

        return Result<ConfidenceScore>.Success(new ConfidenceScore(value));
    }

    public static ConfidenceScore FromStorage(double value)
    {
        return new ConfidenceScore(value);
    }
}
