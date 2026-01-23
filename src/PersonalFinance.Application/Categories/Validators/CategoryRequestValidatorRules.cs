using System.Linq.Expressions;
using FluentValidation;
using PersonalFinance.Shared.Constraints;

namespace PersonalFinance.Application.Categories.Validators;

internal static class CategoryRequestValidatorRules
{
    public static void ApplyCategoryRules<TRequest>(
        AbstractValidator<TRequest> validator,
        Expression<Func<TRequest, string>> nameSelector,
        Expression<Func<TRequest, string>> colorHexSelector)
    {
        var minLength = EntityConstraints.Category.NameMinLength;
        var maxLength = EntityConstraints.Category.NameMaxLength;

        validator.RuleFor(nameSelector)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .Must(name =>
            {
                var trimmed = name.Trim();
                return trimmed.Length >= minLength && trimmed.Length <= maxLength;
            });

        validator.RuleFor(colorHexSelector)
            .NotEmpty()
            .Matches(EntityConstraints.Category.ColorHexRegex);
    }
}
