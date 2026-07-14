namespace FSH.Modules.Avicola.Contracts.Dtos;

/// <summary>
/// Full settlement of a flock (liquidación de la camada): production close-out plus the complete
/// financial result — costs (chicks, feed, health, orders, manual movements) vs revenue (dispatches,
/// manual movements), net result and unit margins.
/// </summary>
public sealed record LiquidacionLoteDto(
    Guid LoteId,
    string Codigo,
    EstadoLote Estado,
    DateTimeOffset FechaIngreso,
    DateTimeOffset? FechaSalidaReal,
    int EdadDias,
    // Producción
    int PollitosEntrantes,
    int PollosSalientes,
    int AvesVivas,
    int TotalBajas,
    decimal PorcentajeMortalidad,
    decimal Viabilidad,
    decimal ConsumoAlimentoKg,
    decimal PesoTotalDespachadoKg,
    decimal? PesoPromedioGramos,
    decimal? ConversionAlimenticia,
    decimal? IndiceEficienciaProductiva,
    // Costos
    decimal CostoPollitos,
    decimal CostoPienso,
    decimal CostoSanidad,
    decimal CostoPedidos,
    decimal CostoMovimientos,
    decimal CostoTotal,
    // Ingresos
    decimal IngresoDespachos,
    decimal IngresoMovimientos,
    decimal IngresoTotal,
    // Resultado
    decimal ResultadoNeto,
    decimal? MargenPorAve,
    decimal? MargenPorKg);
