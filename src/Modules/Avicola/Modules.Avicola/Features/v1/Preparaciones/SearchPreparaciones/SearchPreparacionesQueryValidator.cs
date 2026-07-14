using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Preparaciones;

namespace FSH.Modules.Avicola.Features.v1.Preparaciones.SearchPreparaciones;

public sealed class SearchPreparacionesQueryValidator : AbstractValidator<SearchPreparacionesQuery>
{
    public SearchPreparacionesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
