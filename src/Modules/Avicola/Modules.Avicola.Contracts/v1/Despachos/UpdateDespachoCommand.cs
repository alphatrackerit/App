using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Despachos;

public sealed record UpdateDespachoCommand(
    Guid DespachoId,
    Guid LoteId,
    DateTimeOffset Fecha,
    int Cantidad,
    decimal PesoTotalKg,
    decimal? PrecioPorKg = null,
    Guid? ClienteId = null,
    string? Notas = null) : ICommand<Guid>;
