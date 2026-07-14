using FSH.Modules.Avicola.Contracts;
using FSH.Modules.Avicola.Contracts.Dtos;
using FSH.Modules.Avicola.Contracts.v1.Indicadores;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Indicadores.GetResumenAvicola;

public sealed class GetResumenAvicolaQueryHandler(AvicolaDbContext dbContext)
    : IQueryHandler<GetResumenAvicolaQuery, ResumenAvicolaDto>
{
    public async ValueTask<ResumenAvicolaDto> Handle(GetResumenAvicolaQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var lotes = await dbContext.Lotes.AsNoTracking()
            .OrderByDescending(l => l.FechaIngreso)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var bajasPorLote = (await dbContext.Mortalidades.AsNoTracking()
            .GroupBy(m => m.LoteId)
            .Select(g => new { LoteId = g.Key, Bajas = g.Sum(x => x.Cantidad + (x.Descartes ?? 0)) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false))
            .ToDictionary(x => x.LoteId, x => x.Bajas);

        var consumoPorLote = (await dbContext.Alimentaciones.AsNoTracking()
            .GroupBy(a => a.LoteId)
            .Select(g => new { LoteId = g.Key, Kg = g.Sum(x => x.CantidadKg) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false))
            .ToDictionary(x => x.LoteId, x => x.Kg);

        var despachoPorLote = (await dbContext.Despachos.AsNoTracking()
            .GroupBy(d => d.LoteId)
            .Select(g => new { LoteId = g.Key, Cantidad = g.Sum(x => x.Cantidad), Kg = g.Sum(x => x.PesoTotalKg) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false))
            .ToDictionary(x => x.LoteId, x => (x.Cantidad, x.Kg));

        var pesoPorLote = (await dbContext.Pesos.AsNoTracking()
            .Select(p => new { p.LoteId, p.Fecha, p.PesoPromedioGramos })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false))
            .GroupBy(p => p.LoteId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.Fecha).First().PesoPromedioGramos);

        var now = DateTimeOffset.UtcNow;
        var filas = new List<LoteResumenDto>(lotes.Count);
        int totalAvesVivas = 0;
        decimal consumoTotal = 0m;

        foreach (var lote in lotes)
        {
            int bajas = bajasPorLote.GetValueOrDefault(lote.Id);
            decimal consumoKg = consumoPorLote.GetValueOrDefault(lote.Id);
            (int despCant, decimal despKg) = despachoPorLote.GetValueOrDefault(lote.Id);
            decimal? peso = pesoPorLote.TryGetValue(lote.Id, out var p) ? p : null;

            int avesVivas = Math.Max(0, lote.CantidadInicial - bajas - despCant);
            decimal pctMortalidad = lote.CantidadInicial > 0
                ? (decimal)bajas / lote.CantidadInicial * 100m
                : 0m;
            decimal? fcr = IndicadoresCalculator.ConversionAlimenticia(consumoKg, avesVivas, peso, despKg);

            totalAvesVivas += avesVivas;
            consumoTotal += consumoKg;

            filas.Add(new LoteResumenDto(
                lote.Id,
                lote.Codigo,
                lote.Estado,
                lote.GalponId,
                lote.FechaIngreso,
                IndicadoresCalculator.EdadDias(lote.FechaIngreso, lote.FechaSalidaReal, now),
                lote.CantidadInicial,
                avesVivas,
                decimal.Round(pctMortalidad, 2),
                decimal.Round(consumoKg, 2),
                peso,
                fcr.HasValue ? decimal.Round(fcr.Value, 3) : null));
        }

        int lotesActivos = lotes.Count(l => l.Estado == EstadoLote.EnCrianza);

        return new ResumenAvicolaDto(
            filas,
            lotes.Count,
            lotesActivos,
            totalAvesVivas,
            decimal.Round(consumoTotal, 2));
    }
}
