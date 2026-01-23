using System.Linq.Expressions;
using FluentValidation;
using PersonalFinance.Shared.Constraints;

namespace PersonalFinance.Application.Requests.Expenses.Validators;

internal static class ExpenseRequestValidatorRules
{
    public static void ApplyExpenseRules<TRequest>(
        AbstractValidator<TRequest> validator,
        Expression<Func<TRequest, DateTime>> dateSelector,
        Expression<Func<TRequest, decimal>> amountSelector,
        Expression<Func<TRequest, string?>> descriptionSelector)
    {
        validator.RuleFor(dateSelector)
            .NotEqual(default(DateTime));

        validator.RuleFor(amountSelector)
            .GreaterThan(0);

        validator.RuleFor(descriptionSelector)
            .Must(description =>
            {
                if (description is null)
                {
                    return true;
                }

                return description.Trim().Length <= EntityConstraints.Expense.DescriptionMaxLength;
            });
    }
}
