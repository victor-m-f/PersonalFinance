using FluentValidation;
using PersonalFinance.Application.InvoiceImport.Requests;

namespace PersonalFinance.Application.InvoiceImport.Validators;

public sealed class ImportDocumentRequestValidator : AbstractValidator<ImportDocumentRequest>
{
    public ImportDocumentRequestValidator()
    {
        RuleFor(x => x.SourceFilePath)
            .NotEmpty();

        RuleFor(x => x.OriginalFileName)
            .NotEmpty();
    }
}
