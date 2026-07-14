using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Pedidos;

namespace FSH.Modules.Avicola.Features.v1.Pedidos.DeletePedido;

public sealed class DeletePedidoCommandValidator : AbstractValidator<DeletePedidoCommand>
{
    public DeletePedidoCommandValidator()
    {
        RuleFor(x => x.PedidoId).NotEmpty();
    }
}
