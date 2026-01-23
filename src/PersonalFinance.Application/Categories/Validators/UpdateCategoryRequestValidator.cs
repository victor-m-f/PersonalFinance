using FluentValidation;
using PersonalFinance.Application.Categories.Requests;

namespace PersonalFinance.Application.Categories.Validators;

public sealed class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty();

        CategoryRequestValidatorRules.ApplyCategoryRules(this, request => request.Name, request => request.ColorHex);
    }
}
