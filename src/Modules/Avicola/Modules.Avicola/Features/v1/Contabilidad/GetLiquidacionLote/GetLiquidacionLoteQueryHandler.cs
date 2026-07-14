using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts;
using FSH.Modules.Avicola.Contracts.Dtos;
using FSH.Modules.Avicola.Contracts.v1.Contabilidad;
using FSH.Modules.Avicola.Data;
using FSH.Modules.Avicola.Features.v1.Indicadores;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Contabilidad.GetLiquidacionLote;

public sealed class GetLiquidacionLoteQueryHandler(AvicolaDbContext dbContext)
    : IQueryHandler<GetLiquidacionLoteQuery, LiquidacionLoteDto>
{
    public async ValueTask<LiquidacionLoteDto> Handle(GetLiquidacionLoteQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var lote = await dbContext.Lotes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == query.LoteId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Lote {query.LoteId} not found.");

        int bajas = await dbContext.Mortalidades.AsNoTracking()
            .Where(m => m.LoteId == lote.Id)
            .SumAsync(m => m.Cantidad + (m.Descartes ?? 0), cancellationToken).ConfigureAwait(false);

        decimal consumoKg = await dbContext.Alimentaciones.AsNoTracking()
            .Where(a => a.LoteId == lote.Id).SumAsync(a => a.CantidadKg, cancellationToken).ConfigureAwait(false);

        decimal costoPienso = await dbContext.Alimentaciones.AsNoTracking()
            .Where(a => a.LoteId == lote.Id).SumAsync(a => a.CantidadKg * (a.CostoUnitario ?? 0m), cancellationToken).ConfigureAwait(false);

        decimal costoSanidad = await dbContext.RegistrosSanitarios.AsNoTracking()
            .Where(s => s.LoteId == lote.Id).SumAsync(s => s.Costo ?? 0m, cancellationToken).ConfigureAwait(false);

        int totalDespachado = await dbContext.Despachos.AsNoTracking()
            .Where(d => d.LoteId == lote.Id).SumAsync(d => d.Cantidad, cancellationToken).ConfigureAwait(false);

        decimal despachadoKg = await dbContext.Despachos.AsNoTracking()
            .Where(d => d.LoteId == lote.Id).SumAsync(d => d.PesoTotalKg, cancellationToken).ConfigureAwait(false);

        decimal ingresoDespachos = await dbContext.Despachos.AsNoTracking()
            .Where(d => d.LoteId == lote.Id).SumAsync(d => d.PesoTotalKg * (d.PrecioPorKg ?? 0m), cancellationToken).ConfigureAwait(false);

        decimal costoPedidos = await dbContext.Pedidos.AsNoTracking()
            .Where(p => p.LoteId == lote.Id && p.Estado == EstadoPedido.Recibido)
            .SumAsync(p => p.CostoReal ?? p.CostoEstimado ?? 0m, cancellationToken).ConfigureAwait(false);

        decimal egresoMovimientos = await dbContext.Movimientos.AsNoTracking()
            .Where(m => m.LoteId == lote.Id && m.Tipo == TipoMovimiento.Egreso)
            .SumAsync(m => m.Importe, cancellationToken).ConfigureAwait(false);

        decimal ingresoMovimientos = await dbContext.Movimientos.AsNoTracking()
            .Where(m => m.LoteId == lote.Id && m.Tipo == TipoMovimiento.Ingreso)
            .SumAsync(m => m.Importe, cancellationToken).ConfigureAwait(false);

        decimal? pesoPromedio = await dbContext.Pesos.AsNoTracking()
            .Where(p => p.LoteId == lote.Id)
            .OrderByDescending(p => p.Fecha)
            .Select(p => (decimal?)p.PesoPromedioGramos)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        int avesVivas = Math.Max(0, lote.CantidadInicial - bajas - totalDespachado);
        decimal pctMortalidad = lote.CantidadInicial > 0 ? (decimal)bajas / lote.CantidadInicial * 100m : 0m;
        decimal viabilidad = 100m - pctMortalidad;
        int edadDias = IndicadoresCalculator.EdadDias(lote.FechaIngreso, lote.FechaSalidaReal, DateTimeOffset.UtcNow);
        decimal? fcr = IndicadoresCalculator.ConversionAlimenticia(consumoKg, avesVivas, pesoPromedio, despachadoKg);
        decimal? iep = IndicadoresCalculator.IndiceEficienciaProductiva(viabilidad, pesoPromedio, edadDias, fcr);

        decimal costoPollitos = lote.CantidadInicial * (lote.CostoPolluelo ?? 0m);
        decimal costoTotal = costoPollitos + costoPienso + costoSanidad + costoPedidos + egresoMovimientos;
        decimal ingresoTotal = ingresoDespachos + ingresoMovimientos;
        decimal resultadoNeto = ingresoTotal - costoTotal;

        decimal? margenPorAve = lote.CantidadInicial > 0 ? resultadoNeto / lote.CantidadInicial : null;
        decimal? margenPorKg = despachadoKg > 0 ? resultadoNeto / despachadoKg : null;

        return new LiquidacionLoteDto(
            lote.Id, lote.Codigo, lote.Estado, lote.FechaIngreso, lote.FechaSalidaReal, edadDias,
            lote.CantidadInicial, totalDespachado, avesVivas, bajas,
            decimal.Round(pctMortalidad, 2), decimal.Round(viabilidad, 2),
            decimal.Round(consumoKg, 2), decimal.Round(despachadoKg, 2), pesoPromedio,
            fcr.HasValue ? decimal.Round(fcr.Value, 3) : null,
            iep.HasValue ? decimal.Round(iep.Value, 1) : null,
            decimal.Round(costoPollitos, 2), decimal.Round(costoPienso, 2), decimal.Round(costoSanidad, 2),
            decimal.Round(costoPedidos, 2), decimal.Round(egresoMovimientos, 2), decimal.Round(costoTotal, 2),
            decimal.Round(ingresoDespachos, 2), decimal.Round(ingresoMovimientos, 2), decimal.Round(ingresoTotal, 2),
            decimal.Round(resultadoNeto, 2),
            margenPorAve.HasValue ? decimal.Round(margenPorAve.Value, 2) : null,
            margenPorKg.HasValue ? decimal.Round(margenPorKg.Value, 2) : null);
    }
}
