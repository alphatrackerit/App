using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Preparaciones;

public sealed record CreatePreparacionCommand(
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
    EstadoPreparacion Estado = EstadoPreparacion.EnProceso,
    string? Notas = null) : ICommand<Guid>;
