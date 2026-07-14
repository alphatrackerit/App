namespace FSH.Modules.Avicola.Contracts.Dtos;

public sealed record GalponDto(
    Guid Id,
    string Nombre,
    string? Codigo,
    int Capacidad,
    decimal? SuperficieM2,
    string? Ubicacion,
    bool Activo,
    string? Notas);
