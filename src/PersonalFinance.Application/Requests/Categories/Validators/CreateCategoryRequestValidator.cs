using FluentValidation;

namespace PersonalFinance.Application.Requests.Categories.Validators;

public sealed class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        CategoryRequestValidatorRules.ApplyCategoryRules(this, request => request.Name, request => request.ColorHex);
    }
}
