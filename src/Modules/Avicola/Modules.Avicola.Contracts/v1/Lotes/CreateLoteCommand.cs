using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Lotes;

public sealed record CreateLoteCommand(
    string Codigo,
    Guid? GalponId = null,
    string? Raza = null,
    DateTimeOffset FechaIngreso = default,
    int CantidadInicial = 0,
    decimal? PesoInicialGramos = null,
    DateTimeOffset? FechaSalidaPrevista = null,
    EstadoLote Estado = EstadoLote.EnCrianza,
    Guid? ProveedorId = null,
    decimal? CostoPolluelo = null,
    string? Notas = null) : ICommand<Guid>;
