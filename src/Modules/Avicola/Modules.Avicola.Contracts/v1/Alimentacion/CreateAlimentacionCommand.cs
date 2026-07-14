using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Alimentacion;

public sealed record CreateAlimentacionCommand(
    Guid LoteId,
    DateTimeOffset Fecha,
    TipoAlimento TipoAlimento,
    decimal CantidadKg,
    decimal? CostoUnitario = null,
    string? Notas = null) : ICommand<Guid>;
