using FluentValidation;
using PersonalFinance.Application.InvoiceImport.Requests;

namespace PersonalFinance.Application.InvoiceImport.Validators;

public sealed class AddVendorCategoryRuleRequestValidator : AbstractValidator<AddVendorCategoryRuleRequest>
{
    public AddVendorCategoryRuleRequestValidator()
    {
        RuleFor(x => x.Keyword)
            .NotEmpty();

        RuleFor(x => x.CategoryId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.Confidence)
            .InclusiveBetween(0d, 1d);
    }
}
