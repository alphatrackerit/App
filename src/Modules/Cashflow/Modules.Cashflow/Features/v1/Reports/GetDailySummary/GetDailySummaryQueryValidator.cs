using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Reports;

namespace FSH.Modules.Cashflow.Features.v1.Reports.GetDailySummary;

public sealed class GetDailySummaryQueryValidator : AbstractValidator<GetDailySummaryQuery>
{
    public GetDailySummaryQueryValidator()
    {
        RuleFor(x => x.To)
            .GreaterThanOrEqualTo(x => x.From!.Value)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("'To' must be on or after 'From'.");
    }
}
