using PersonalFinance.Shared.Results;

namespace PersonalFinance.Domain.ValueObjects;

public sealed record class CategoryColor
{
    public string Value { get; }

    private CategoryColor(string value)
    {
        Value = value;
    }

    public static Result<CategoryColor> Create(string colorHex)
    {
        if (string.IsNullOrWhiteSpace(colorHex))
        {
            return Result<CategoryColor>.Failure(Errors.ValidationError, "Color is required.");
        }

        var trimmed = colorHex.Trim();

        if (!IsValid(trimmed))
        {
            return Result<CategoryColor>.Failure(Errors.ValidationError, "Color must be in format #AARRGGBB.");
        }

        return Result<CategoryColor>.Success(new CategoryColor(trimmed));
    }

    private static bool IsValid(string value)
    {
        if (value.Length != 9)
        {
            return false;
        }

        if (value[0] != '#')
        {
            return false;
        }

        for (var i = 1; i < value.Length; i++)
        {
            if (!Uri.IsHexDigit(value[i]))
            {
                return false;
            }
        }

        return true;
    }
}
