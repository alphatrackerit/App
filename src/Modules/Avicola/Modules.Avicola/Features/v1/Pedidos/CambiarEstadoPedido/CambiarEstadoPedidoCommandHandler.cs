using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts;
using FSH.Modules.Avicola.Contracts.v1.Pedidos;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Pedidos.CambiarEstadoPedido;

public sealed class CambiarEstadoPedidoCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<CambiarEstadoPedidoCommand, Guid>
{
    public async ValueTask<Guid> Handle(CambiarEstadoPedidoCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var pedido = await dbContext.Pedidos
            .FirstOrDefaultAsync(p => p.Id == command.PedidoId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Pedido {command.PedidoId} not found.");

        switch (command.Accion)
        {
            case AccionPedido.Enviar:
                pedido.Enviar();
                break;
            case AccionPedido.Recibir:
                pedido.Recibir(command.FechaRecepcion ?? DateTimeOffset.UtcNow, command.CostoReal);
                break;
            case AccionPedido.Cancelar:
                pedido.Cancelar();
                break;
            default:
                throw new CustomException($"Unknown order action '{command.Accion}'.");
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return pedido.Id;
    }
}
