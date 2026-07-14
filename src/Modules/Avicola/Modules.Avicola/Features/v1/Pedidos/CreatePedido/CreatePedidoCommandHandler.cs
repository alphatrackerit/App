using FSH.Modules.Avicola.Contracts.v1.Pedidos;
using FSH.Modules.Avicola.Data;
using FSH.Modules.Avicola.Domain;
using Mediator;

namespace FSH.Modules.Avicola.Features.v1.Pedidos.CreatePedido;

public sealed class CreatePedidoCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<CreatePedidoCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreatePedidoCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var pedido = Pedido.Create(
            command.Codigo,
            command.Tipo,
            command.ProveedorId,
            command.Descripcion,
            command.Cantidad,
            command.Unidad,
            command.CostoEstimado,
            command.Estado,
            command.FechaPedido == default ? DateTimeOffset.UtcNow : command.FechaPedido,
            command.LoteId,
            command.GalponId,
            command.Notas);

        dbContext.Pedidos.Add(pedido);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return pedido.Id;
    }
}
