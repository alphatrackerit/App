using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.SearchProformas;

public sealed class SearchProformasQueryValidator : AbstractValidator<SearchProformasQuery>
{
    public SearchProformasQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 10000);
    }
}
