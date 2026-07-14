using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Mortalidad;

namespace FSH.Modules.Avicola.Features.v1.Mortalidad.SearchMortalidad;

public sealed class SearchMortalidadQueryValidator : AbstractValidator<SearchMortalidadQuery>
{
    public SearchMortalidadQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
