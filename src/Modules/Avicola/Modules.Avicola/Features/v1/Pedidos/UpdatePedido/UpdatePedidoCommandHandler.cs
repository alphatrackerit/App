using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Pedidos;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Pedidos.UpdatePedido;

public sealed class UpdatePedidoCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<UpdatePedidoCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdatePedidoCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var pedido = await dbContext.Pedidos
            .FirstOrDefaultAsync(p => p.Id == command.PedidoId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Pedido {command.PedidoId} not found.");

        pedido.Update(
            command.Codigo,
            command.Tipo,
            command.ProveedorId,
            command.Descripcion,
            command.Cantidad,
            command.Unidad,
            command.CostoEstimado,
            command.LoteId,
            command.GalponId,
            command.Notas);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return pedido.Id;
    }
}
