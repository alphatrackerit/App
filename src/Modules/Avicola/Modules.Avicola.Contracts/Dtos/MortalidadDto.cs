namespace FSH.Modules.Avicola.Contracts.Dtos;

public sealed record MortalidadDto(
    Guid Id,
    Guid LoteId,
    DateTimeOffset Fecha,
    int Cantidad,
    int? Descartes,
    string? Causa,
    string? Notas);
