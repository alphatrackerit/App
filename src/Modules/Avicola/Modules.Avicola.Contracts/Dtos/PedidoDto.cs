namespace FSH.Modules.Avicola.Contracts.Dtos;

public sealed record PedidoDto(
    Guid Id,
    string Codigo,
    TipoPedido Tipo,
    Guid? ProveedorId,
    string? Descripcion,
    decimal Cantidad,
    string? Unidad,
    decimal? CostoEstimado,
    decimal? CostoReal,
    EstadoPedido Estado,
    DateTimeOffset FechaPedido,
    DateTimeOffset? FechaRecepcion,
    Guid? LoteId,
    Guid? GalponId,
    string? Notas);
