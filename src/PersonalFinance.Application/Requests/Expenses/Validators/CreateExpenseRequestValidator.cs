using FluentValidation;

namespace PersonalFinance.Application.Requests.Expenses.Validators;

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
