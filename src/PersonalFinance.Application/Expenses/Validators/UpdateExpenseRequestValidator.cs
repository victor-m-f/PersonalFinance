using FluentValidation;
using PersonalFinance.Application.Expenses.Requests;

namespace PersonalFinance.Application.Expenses.Validators;

public sealed class UpdateExpenseRequestValidator : AbstractValidator<UpdateExpenseRequest>
{
    public UpdateExpenseRequestValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty();

        ExpenseRequestValidatorRules.ApplyExpenseRules(
            this,
            request => request.Date,
            request => request.Amount,
            request => request.Description);
    }
}
