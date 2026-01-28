using FluentValidation;
using PersonalFinance.Application.InvoiceImport.Requests;

namespace PersonalFinance.Application.InvoiceImport.Validators;

public sealed class ConfirmImportRequestValidator : AbstractValidator<ConfirmImportRequest>
{
    public ConfirmImportRequestValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.Items)
            .NotEmpty();

        RuleForEach(x => x.Items)
            .SetValidator(new ExpenseDraftItemRequestValidator());
    }
}
