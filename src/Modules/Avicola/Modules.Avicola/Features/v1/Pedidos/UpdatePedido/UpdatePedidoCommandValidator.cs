using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Pedidos;

namespace FSH.Modules.Avicola.Features.v1.Pedidos.UpdatePedido;

public sealed class UpdatePedidoCommandValidator : AbstractValidator<UpdatePedidoCommand>
{
    public UpdatePedidoCommandValidator()
    {
        RuleFor(x => x.PedidoId).NotEmpty();
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Cantidad).GreaterThanOrEqualTo(0);
    }
}
