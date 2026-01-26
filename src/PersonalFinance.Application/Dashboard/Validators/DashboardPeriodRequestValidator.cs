using FluentValidation;
using PersonalFinance.Application.Dashboard.Requests;

namespace PersonalFinance.Application.Dashboard.Validators;

public sealed class DashboardPeriodRequestValidator : AbstractValidator<DashboardPeriodRequest>
{
    public DashboardPeriodRequestValidator()
    {
        RuleFor(x => x.StartDate)
            .NotNull()
            .When(x => x.EndDate.HasValue);

        RuleFor(x => x.EndDate)
            .NotNull()
            .When(x => x.StartDate.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.Month)
            .NotNull()
            .When(x => !x.StartDate.HasValue && !x.EndDate.HasValue);

        RuleFor(x => x.Year)
            .NotNull()
            .When(x => !x.StartDate.HasValue && !x.EndDate.HasValue);

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .When(x => x.Month.HasValue);

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100)
            .When(x => x.Year.HasValue);
    }
}
