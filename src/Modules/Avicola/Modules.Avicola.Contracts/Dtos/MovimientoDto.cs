namespace FSH.Modules.Avicola.Contracts.Dtos;

public sealed record MovimientoDto(
    Guid Id,
    DateTimeOffset Fecha,
    TipoMovimiento Tipo,
    CategoriaMovimiento Categoria,
    string Concepto,
    decimal Importe,
    Guid? LoteId,
    string? Notas);
