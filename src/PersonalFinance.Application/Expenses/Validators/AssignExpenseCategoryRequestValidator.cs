using FluentValidation;
using PersonalFinance.Application.Expenses.Requests;

namespace PersonalFinance.Application.Expenses.Validators;

public sealed class AssignExpenseCategoryRequestValidator : AbstractValidator<AssignExpenseCategoryRequest>
{
    public AssignExpenseCategoryRequestValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty();
    }
}
