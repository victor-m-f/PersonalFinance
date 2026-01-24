using FluentValidation;
using PersonalFinance.Application.Expenses.Requests;

namespace PersonalFinance.Application.Expenses.Validators;

public sealed class DeleteExpenseRequestValidator : AbstractValidator<DeleteExpenseRequest>
{
    public DeleteExpenseRequestValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty();
    }
}
