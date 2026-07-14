using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Movimientos;

namespace FSH.Modules.Avicola.Features.v1.Movimientos.SearchMovimientos;

public sealed class SearchMovimientosQueryValidator : AbstractValidator<SearchMovimientosQuery>
{
    public SearchMovimientosQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
