using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Pedidos;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Pedidos.DeletePedido;

public sealed class DeletePedidoCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<DeletePedidoCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeletePedidoCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var pedido = await dbContext.Pedidos
            .FirstOrDefaultAsync(p => p.Id == command.PedidoId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Pedido {command.PedidoId} not found.");

        dbContext.Pedidos.Remove(pedido);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
