using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Pedidos;

namespace FSH.Modules.Avicola.Features.v1.Pedidos.SearchPedidos;

public sealed class SearchPedidosQueryValidator : AbstractValidator<SearchPedidosQuery>
{
    public SearchPedidosQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
