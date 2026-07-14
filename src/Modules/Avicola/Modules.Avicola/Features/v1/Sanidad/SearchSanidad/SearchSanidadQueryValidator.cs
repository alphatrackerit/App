using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Sanidad;

namespace FSH.Modules.Avicola.Features.v1.Sanidad.SearchSanidad;

public sealed class SearchSanidadQueryValidator : AbstractValidator<SearchSanidadQuery>
{
    public SearchSanidadQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
