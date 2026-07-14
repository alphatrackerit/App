using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Pedidos;

/// <summary>
/// Applies a lifecycle transition to a purchase order: <c>Enviar</c>, <c>Recibir</c> (optionally
/// with reception date + real cost) or <c>Cancelar</c>.
/// </summary>
public sealed record CambiarEstadoPedidoCommand(
    Guid PedidoId,
    AccionPedido Accion,
    DateTimeOffset? FechaRecepcion = null,
    decimal? CostoReal = null) : ICommand<Guid>;
