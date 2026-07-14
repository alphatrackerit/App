using FSH.Modules.Avicola.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Pedidos;

public sealed record GetPedidoByIdQuery(Guid PedidoId) : IQuery<PedidoDto>;
