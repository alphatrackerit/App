using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Pedidos;

namespace FSH.Modules.Avicola.Features.v1.Pedidos.CreatePedido;

public sealed class CreatePedidoCommandValidator : AbstractValidator<CreatePedidoCommand>
{
    public CreatePedidoCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Cantidad).GreaterThanOrEqualTo(0);
    }
}
