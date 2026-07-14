namespace FSH.Modules.Avicola.Contracts.Dtos;

public sealed record PreparacionDto(
    Guid Id,
    Guid GalponId,
    Guid? LoteAnteriorId,
    DateTimeOffset? FechaRetiro,
    DateTimeOffset FechaInicio,
    DateTimeOffset? FechaFin,
    bool RetiradaCama,
    bool Lavado,
    bool Desinfeccion,
    bool Desinsectacion,
    bool CamaNueva,
    decimal? Costo,
    EstadoPreparacion Estado,
    string? Notas);
