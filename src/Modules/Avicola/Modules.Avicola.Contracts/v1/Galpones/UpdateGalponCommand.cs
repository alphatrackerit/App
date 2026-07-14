using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Galpones;

public sealed record UpdateGalponCommand(
    Guid GalponId,
    string Nombre,
    string? Codigo = null,
    int Capacidad = 0,
    decimal? SuperficieM2 = null,
    string? Ubicacion = null,
    bool Activo = true,
    string? Notas = null) : ICommand<Guid>;
