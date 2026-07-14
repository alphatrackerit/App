using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.Dtos;
using FSH.Modules.Avicola.Contracts.v1.Indicadores;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Indicadores.GetIndicadoresLote;

public sealed class GetIndicadoresLoteQueryHandler(AvicolaDbContext dbContext)
    : IQueryHandler<GetIndicadoresLoteQuery, IndicadoresLoteDto>
{
    public async ValueTask<IndicadoresLoteDto> Handle(GetIndicadoresLoteQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var lote = await dbContext.Lotes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == query.LoteId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Lote {query.LoteId} not found.");

        int bajas = await dbContext.Mortalidades.AsNoTracking()
            .Where(m => m.LoteId == lote.Id)
            .SumAsync(m => m.Cantidad + (m.Descartes ?? 0), cancellationToken)
            .ConfigureAwait(false);

        decimal consumoKg = await dbContext.Alimentaciones.AsNoTracking()
            .Where(a => a.LoteId == lote.Id)
            .SumAsync(a => a.CantidadKg, cancellationToken)
            .ConfigureAwait(false);

        decimal costoAlimento = await dbContext.Alimentaciones.AsNoTracking()
            .Where(a => a.LoteId == lote.Id)
            .SumAsync(a => a.CantidadKg * (a.CostoUnitario ?? 0m), cancellationToken)
            .ConfigureAwait(false);

        decimal costoSanidad = await dbContext.RegistrosSanitarios.AsNoTracking()
            .Where(s => s.LoteId == lote.Id)
            .SumAsync(s => s.Costo ?? 0m, cancellationToken)
            .ConfigureAwait(false);

        int totalDespachado = await dbContext.Despachos.AsNoTracking()
            .Where(d => d.LoteId == lote.Id)
            .SumAsync(d => d.Cantidad, cancellationToken)
            .ConfigureAwait(false);

        decimal despachadoKg = await dbContext.Despachos.AsNoTracking()
            .Where(d => d.LoteId == lote.Id)
            .SumAsync(d => d.PesoTotalKg, cancellationToken)
            .ConfigureAwait(false);

        decimal ingresoDespachos = await dbContext.Despachos.AsNoTracking()
            .Where(d => d.LoteId == lote.Id)
            .SumAsync(d => d.PesoTotalKg * (d.PrecioPorKg ?? 0m), cancellationToken)
            .ConfigureAwait(false);

        decimal? pesoPromedio = await dbContext.Pesos.AsNoTracking()
            .Where(p => p.LoteId == lote.Id)
            .OrderByDescending(p => p.Fecha)
            .Select(p => (decimal?)p.PesoPromedioGramos)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        int avesVivas = Math.Max(0, lote.CantidadInicial - bajas - totalDespachado);
        decimal pctMortalidad = lote.CantidadInicial > 0
            ? (decimal)bajas / lote.CantidadInicial * 100m
            : 0m;
        decimal viabilidad = 100m - pctMortalidad;
        int edadDias = IndicadoresCalculator.EdadDias(lote.FechaIngreso, lote.FechaSalidaReal, DateTimeOffset.UtcNow);

        decimal? gananciaDiaria = null;
        if (pesoPromedio is > 0 && edadDias > 0)
        {
            decimal pesoInicial = lote.PesoInicialGramos ?? 0m;
            gananciaDiaria = (pesoPromedio.Value - pesoInicial) / edadDias;
        }

        decimal? fcr = IndicadoresCalculator.ConversionAlimenticia(consumoKg, avesVivas, pesoPromedio, despachadoKg);
        decimal? iep = IndicadoresCalculator.IndiceEficienciaProductiva(viabilidad, pesoPromedio, edadDias, fcr);

        decimal costoPolluelos = lote.CantidadInicial * (lote.CostoPolluelo ?? 0m);
        decimal costoTotal = costoPolluelos + costoAlimento + costoSanidad;

        return new IndicadoresLoteDto(
            lote.Id,
            lote.Codigo,
            lote.Estado,
            lote.FechaIngreso,
            edadDias,
            lote.CantidadInicial,
            bajas,
            totalDespachado,
            avesVivas,
            decimal.Round(pctMortalidad, 2),
            decimal.Round(viabilidad, 2),
            decimal.Round(consumoKg, 2),
            pesoPromedio,
            gananciaDiaria.HasValue ? decimal.Round(gananciaDiaria.Value, 2) : null,
            fcr.HasValue ? decimal.Round(fcr.Value, 3) : null,
            iep.HasValue ? decimal.Round(iep.Value, 1) : null,
            decimal.Round(costoPolluelos, 2),
            decimal.Round(costoAlimento, 2),
            decimal.Round(costoSanidad, 2),
            decimal.Round(costoTotal, 2),
            decimal.Round(ingresoDespachos, 2));
    }
}
