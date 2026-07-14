using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Pedidos;

namespace FSH.Modules.Avicola.Features.v1.Pedidos.CambiarEstadoPedido;

public sealed class CambiarEstadoPedidoCommandValidator : AbstractValidator<CambiarEstadoPedidoCommand>
{
    public CambiarEstadoPedidoCommandValidator()
    {
        RuleFor(x => x.PedidoId).NotEmpty();
    }
}
