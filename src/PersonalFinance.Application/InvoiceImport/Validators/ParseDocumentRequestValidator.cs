using FluentValidation;
using PersonalFinance.Application.InvoiceImport.Requests;

namespace PersonalFinance.Application.InvoiceImport.Validators;

public sealed class ParseDocumentRequestValidator : AbstractValidator<ParseDocumentRequest>
{
    public ParseDocumentRequestValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEqual(Guid.Empty);
    }
}
