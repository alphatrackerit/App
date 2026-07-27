using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Reports;

namespace FSH.Modules.Cashflow.Features.v1.Reports.GetInvoicesReport;

public sealed class GetInvoicesReportQueryValidator : AbstractValidator<GetInvoicesReportQuery>
{
    public GetInvoicesReportQueryValidator()
    {
        RuleFor(x => x.Scope).IsInEnum();
        RuleFor(x => x.Type).IsInEnum().When(x => x.Type.HasValue);
        RuleFor(x => x.DueWithinDays).InclusiveBetween(1, 3650).When(x => x.DueWithinDays.HasValue);
        RuleFor(x => x.To)
            .GreaterThanOrEqualTo(x => x.From)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("'To' debe ser posterior o igual a 'From'.");
    }
}
