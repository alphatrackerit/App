namespace FSH.Modules.Avicola.Contracts.Dtos;

public sealed record LoteDto(
    Guid Id,
    string Codigo,
    Guid? GalponId,
    string? Raza,
    DateTimeOffset FechaIngreso,
    int CantidadInicial,
    decimal? PesoInicialGramos,
    DateTimeOffset? FechaSalidaPrevista,
    DateTimeOffset? FechaSalidaReal,
    EstadoLote Estado,
    Guid? ProveedorId,
    decimal? CostoPolluelo,
    string? Notas);
