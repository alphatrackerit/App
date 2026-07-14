namespace FSH.Modules.Avicola.Features.v1.Indicadores;

/// <summary>
/// Shared broiler-performance maths so the per-flock and dashboard handlers compute identical KPIs.
/// All weights are handled in grams at the boundary; conversions to kg happen here.
/// </summary>
internal static class IndicadoresCalculator
{
    /// <summary>Age in days from placement to exit (or now, for an open flock).</summary>
    internal static int EdadDias(DateTimeOffset ingreso, DateTimeOffset? salida, DateTimeOffset now)
    {
        var fin = salida ?? now;
        var dias = (int)Math.Floor((fin - ingreso).TotalDays);
        return dias < 0 ? 0 : dias;
    }

    /// <summary>
    /// Feed Conversion Ratio: feed consumed (kg) / live weight produced (kg), where produced biomass
    /// is current live birds plus everything already dispatched. Null when there is no biomass yet.
    /// </summary>
    internal static decimal? ConversionAlimenticia(decimal consumoKg, int avesVivas, decimal? pesoGramos, decimal despachadoKg)
    {
        decimal biomasaKg = despachadoKg;
        if (pesoGramos is > 0 && avesVivas > 0)
        {
            biomasaKg += avesVivas * (pesoGramos.Value / 1000m);
        }

        return biomasaKg > 0 ? consumoKg / biomasaKg : null;
    }

    /// <summary>
    /// European Production Efficiency Factor (IEP / EPEF):
    /// (viability% × live weight kg) / (age days × FCR) × 100. Null when any input is missing.
    /// </summary>
    internal static decimal? IndiceEficienciaProductiva(decimal viabilidad, decimal? pesoGramos, int edadDias, decimal? fcr)
    {
        if (pesoGramos is not > 0 || edadDias <= 0 || fcr is not > 0)
        {
            return null;
        }

        decimal pesoKg = pesoGramos.Value / 1000m;
        return viabilidad * pesoKg / (edadDias * fcr.Value) * 100m;
    }
}
