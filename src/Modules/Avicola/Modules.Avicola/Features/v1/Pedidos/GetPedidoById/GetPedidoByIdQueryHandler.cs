using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.Dtos;
using FSH.Modules.Avicola.Contracts.v1.Pedidos;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Pedidos.GetPedidoById;

public sealed class GetPedidoByIdQueryHandler(AvicolaDbContext dbContext)
    : IQueryHandler<GetPedidoByIdQuery, PedidoDto>
{
    public async ValueTask<PedidoDto> Handle(GetPedidoByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var x = await dbContext.Pedidos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.PedidoId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Pedido {query.PedidoId} not found.");

        return new PedidoDto(
            x.Id, x.Codigo, x.Tipo, x.ProveedorId, x.Descripcion, x.Cantidad, x.Unidad, x.CostoEstimado,
            x.CostoReal, x.Estado, x.FechaPedido, x.FechaRecepcion, x.LoteId, x.GalponId, x.Notas);
    }
}
