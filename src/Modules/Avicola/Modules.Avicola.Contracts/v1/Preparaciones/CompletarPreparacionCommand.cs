using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Preparaciones;

/// <summary>Marks a shed preparation complete (the nave is ready for the next flock).</summary>
public sealed record CompletarPreparacionCommand(
    Guid PreparacionId,
    DateTimeOffset? FechaFin = null) : ICommand<Guid>;
