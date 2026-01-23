using FluentValidation;
using PersonalFinance.Application.Expenses.Requests;

namespace PersonalFinance.Application.Expenses.Validators;

public sealed class CreateExpenseRequestValidator : AbstractValidator<CreateExpenseRequest>
{
    public CreateExpenseRequestValidator()
    {
        ExpenseRequestValidatorRules.ApplyExpenseRules(
            this,
            request => request.Date,
            request => request.Amount,
            request => request.Description);
    }
}
