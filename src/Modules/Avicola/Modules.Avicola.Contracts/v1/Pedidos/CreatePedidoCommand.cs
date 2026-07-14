using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Pedidos;

public sealed record CreatePedidoCommand(
    string Codigo,
    TipoPedido Tipo,
    Guid? ProveedorId = null,
    string? Descripcion = null,
    decimal Cantidad = 0,
    string? Unidad = null,
    decimal? CostoEstimado = null,
    EstadoPedido Estado = EstadoPedido.Borrador,
    DateTimeOffset FechaPedido = default,
    Guid? LoteId = null,
    Guid? GalponId = null,
    string? Notas = null) : ICommand<Guid>;
