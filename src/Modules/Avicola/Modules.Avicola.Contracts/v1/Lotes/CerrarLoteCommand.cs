using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Lotes;

/// <summary>Closes a flock: records the actual exit date and marks it <c>Finalizado</c>.</summary>
public sealed record CerrarLoteCommand(
    Guid LoteId,
    DateTimeOffset? FechaSalidaReal = null) : ICommand<Guid>;
