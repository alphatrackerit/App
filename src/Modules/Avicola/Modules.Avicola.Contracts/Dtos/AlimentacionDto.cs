namespace FSH.Modules.Avicola.Contracts.Dtos;

public sealed record AlimentacionDto(
    Guid Id,
    Guid LoteId,
    DateTimeOffset Fecha,
    TipoAlimento TipoAlimento,
    decimal CantidadKg,
    decimal? CostoUnitario,
    string? Notas);
