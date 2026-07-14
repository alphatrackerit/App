namespace FSH.Modules.Avicola.Contracts.Dtos;

public sealed record DespachoDto(
    Guid Id,
    Guid LoteId,
    DateTimeOffset Fecha,
    int Cantidad,
    decimal PesoTotalKg,
    decimal? PrecioPorKg,
    Guid? ClienteId,
    string? Notas);
