using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Despachos;

namespace FSH.Modules.Avicola.Features.v1.Despachos.SearchDespachos;

public sealed class SearchDespachosQueryValidator : AbstractValidator<SearchDespachosQuery>
{
    public SearchDespachosQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
