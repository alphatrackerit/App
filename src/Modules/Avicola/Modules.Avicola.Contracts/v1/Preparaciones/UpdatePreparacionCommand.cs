using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Preparaciones;

public sealed record UpdatePreparacionCommand(
    Guid PreparacionId,
    Guid GalponId,
    Guid? LoteAnteriorId = null,
    DateTimeOffset? FechaRetiro = null,
    DateTimeOffset FechaInicio = default,
    bool RetiradaCama = false,
    bool Lavado = false,
    bool Desinfeccion = false,
    bool Desinsectacion = false,
    bool CamaNueva = false,
    decimal? Costo = null,
    string? Notas = null) : ICommand<Guid>;
