using FluentValidation;
using PersonalFinance.Application.Categories.Requests;

namespace PersonalFinance.Application.Categories.Validators;

public sealed class DeleteCategoryRequestValidator : AbstractValidator<DeleteCategoryRequest>
{
    public DeleteCategoryRequestValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty();
    }
}
