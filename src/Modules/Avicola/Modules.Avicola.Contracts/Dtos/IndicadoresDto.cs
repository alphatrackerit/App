namespace FSH.Modules.Avicola.Contracts.Dtos;

/// <summary>
/// Performance indicators (KPIs) for a single broiler flock — the production efficiency dashboard.
/// Computed from the flock's mortality, feed, weight and dispatch records.
/// </summary>
public sealed record IndicadoresLoteDto(
    Guid LoteId,
    string Codigo,
    EstadoLote Estado,
    DateTimeOffset FechaIngreso,
    int EdadDias,
    int CantidadInicial,
    int TotalBajas,
    int TotalDespachado,
    int AvesVivas,
    decimal PorcentajeMortalidad,
    decimal Viabilidad,
    decimal ConsumoAlimentoKg,
    decimal? PesoPromedioGramos,
    decimal? GananciaDiariaGramos,
    decimal? ConversionAlimenticia,
    decimal? IndiceEficienciaProductiva,
    decimal CostoPolluelos,
    decimal CostoAlimento,
    decimal CostoSanidad,
    decimal CostoTotal,
    decimal IngresoDespachos);

/// <summary>Headline metrics for one flock, for the cross-flock dashboard summary grid.</summary>
public sealed record LoteResumenDto(
    Guid Id,
    string Codigo,
    EstadoLote Estado,
    Guid? GalponId,
    DateTimeOffset FechaIngreso,
    int EdadDias,
    int CantidadInicial,
    int AvesVivas,
    decimal PorcentajeMortalidad,
    decimal ConsumoAlimentoKg,
    decimal? PesoPromedioGramos,
    decimal? ConversionAlimenticia);

/// <summary>
/// Cross-flock dashboard summary: one row per flock plus tenant-wide rollups, so the dashboard
/// renders the flock table and headline cards/charts from a single query.
/// </summary>
public sealed record ResumenAvicolaDto(
    IReadOnlyList<LoteResumenDto> Lotes,
    int TotalLotes,
    int LotesActivos,
    int TotalAvesVivas,
    decimal ConsumoAlimentoTotalKg);
