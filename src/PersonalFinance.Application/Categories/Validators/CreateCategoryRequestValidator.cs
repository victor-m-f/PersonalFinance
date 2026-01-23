using FluentValidation;
using PersonalFinance.Application.Categories.Requests;

namespace PersonalFinance.Application.Categories.Validators;

public sealed class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        CategoryRequestValidatorRules.ApplyCategoryRules(this, request => request.Name, request => request.ColorHex);
    }
}
