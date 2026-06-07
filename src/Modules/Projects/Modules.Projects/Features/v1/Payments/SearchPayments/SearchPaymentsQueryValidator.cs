using FluentValidation;
using FSH.Modules.Projects.Contracts.v1.Payments;

namespace FSH.Modules.Projects.Features.v1.Payments.SearchPayments;

public sealed class SearchPaymentsQueryValidator : AbstractValidator<SearchPaymentsQuery>
{
    public SearchPaymentsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
