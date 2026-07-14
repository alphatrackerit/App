namespace FSH.Modules.Avicola.Contracts.Dtos;

/// <summary>Income/expense rolled up for one accounting category.</summary>
public sealed record ContabilidadCategoriaDto(
    CategoriaMovimiento Categoria,
    decimal Ingresos,
    decimal Egresos);

/// <summary>Per-flock financial roll-up (computed costs + revenue + manual movements).</summary>
public sealed record ContabilidadLoteDto(
    Guid LoteId,
    string Codigo,
    EstadoLote Estado,
    decimal Costos,
    decimal Ingresos,
    decimal Resultado);

/// <summary>
/// Accounting overview within Avícola: computed costs (chicks, feed, health, received orders) and
/// revenue (dispatches) plus manual ledger movements, broken down by category and by flock.
/// </summary>
public sealed record ContabilidadDto(
    IReadOnlyList<ContabilidadCategoriaDto> PorCategoria,
    IReadOnlyList<ContabilidadLoteDto> PorLote,
    decimal TotalIngresos,
    decimal TotalEgresos,
    decimal Resultado);
