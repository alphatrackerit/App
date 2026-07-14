using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Movimientos;

public sealed record CreateMovimientoCommand(
    DateTimeOffset Fecha,
    TipoMovimiento Tipo,
    CategoriaMovimiento Categoria,
    string Concepto,
    decimal Importe,
    Guid? LoteId = null,
    string? Notas = null) : ICommand<Guid>;
