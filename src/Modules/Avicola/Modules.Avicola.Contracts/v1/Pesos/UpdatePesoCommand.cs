using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Pesos;

public sealed record UpdatePesoCommand(
    Guid PesoId,
    Guid LoteId,
    DateTimeOffset Fecha,
    decimal PesoPromedioGramos,
    int? CantidadMuestra = null,
    string? Notas = null) : ICommand<Guid>;
