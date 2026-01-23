using FluentValidation;

namespace PersonalFinance.Application.Requests.Categories.Validators;

public sealed class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty();

        CategoryRequestValidatorRules.ApplyCategoryRules(this, request => request.Name, request => request.ColorHex);
    }
}
