using FluentValidation;
using PersonalFinance.Application.InvoiceImport.Requests;

namespace PersonalFinance.Application.InvoiceImport.Validators;

public sealed class ReviewDraftRequestValidator : AbstractValidator<ReviewDraftRequest>
{
    public ReviewDraftRequestValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEqual(Guid.Empty);

        RuleForEach(x => x.Items)
            .SetValidator(new ExpenseDraftItemRequestValidator());
    }
}
