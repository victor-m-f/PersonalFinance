using FluentValidation;
using PersonalFinance.Application.InvoiceImport.Requests;

namespace PersonalFinance.Application.InvoiceImport.Validators;

public sealed class ExpenseDraftItemRequestValidator : AbstractValidator<ExpenseDraftItemRequest>
{
    public ExpenseDraftItemRequestValidator()
    {
        RuleFor(x => x.Date)
            .NotEqual(default(DateTime));

        RuleFor(x => x.Amount)
            .GreaterThan(0m);

        RuleFor(x => x.Confidence)
            .InclusiveBetween(0d, 1d);
    }
}
