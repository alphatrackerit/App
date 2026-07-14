namespace FSH.Modules.Avicola.Contracts.Dtos;

public sealed record PesoDto(
    Guid Id,
    Guid LoteId,
    DateTimeOffset Fecha,
    decimal PesoPromedioGramos,
    int? CantidadMuestra,
    string? Notas);
