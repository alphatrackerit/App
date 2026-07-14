using FSH.Modules.Avicola.Contracts;
using FSH.Modules.Avicola.Contracts.Dtos;
using FSH.Modules.Avicola.Contracts.v1.Contabilidad;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Contabilidad.GetContabilidad;

public sealed class GetContabilidadQueryHandler(AvicolaDbContext dbContext)
    : IQueryHandler<GetContabilidadQuery, ContabilidadDto>
{
    public async ValueTask<ContabilidadDto> Handle(GetContabilidadQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var loteId = query.LoteId;
        var desde = query.Desde;
        var hasta = query.Hasta;
        bool InRange(DateTimeOffset f) => (desde is null || f >= desde) && (hasta is null || f <= hasta);

        var lotes = await dbContext.Lotes.AsNoTracking()
            .Where(l => loteId == null || l.Id == loteId)
            .Select(l => new { l.Id, l.Codigo, l.Estado, l.CantidadInicial, l.CostoPolluelo, l.FechaIngreso })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var alim = await dbContext.Alimentaciones.AsNoTracking()
            .Where(a => loteId == null || a.LoteId == loteId)
            .Select(a => new { a.LoteId, a.Fecha, Costo = a.CantidadKg * (a.CostoUnitario ?? 0m) })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var san = await dbContext.RegistrosSanitarios.AsNoTracking()
            .Where(s => loteId == null || s.LoteId == loteId)
            .Select(s => new { s.LoteId, s.Fecha, Costo = s.Costo ?? 0m })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var desp = await dbContext.Despachos.AsNoTracking()
            .Where(d => loteId == null || d.LoteId == loteId)
            .Select(d => new { d.LoteId, d.Fecha, Ingreso = d.PesoTotalKg * (d.PrecioPorKg ?? 0m) })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var ped = await dbContext.Pedidos.AsNoTracking()
            .Where(p => (loteId == null || p.LoteId == loteId) && p.Estado == EstadoPedido.Recibido)
            .Select(p => new { p.LoteId, p.Tipo, Fecha = p.FechaRecepcion ?? p.FechaPedido, Costo = p.CostoReal ?? p.CostoEstimado ?? 0m })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var movs = await dbContext.Movimientos.AsNoTracking()
            .Where(m => loteId == null || m.LoteId == loteId)
            .Select(m => new { m.LoteId, m.Fecha, m.Tipo, m.Categoria, m.Importe })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var ingresosPorCat = new Dictionary<CategoriaMovimiento, decimal>();
        var egresosPorCat = new Dictionary<CategoriaMovimiento, decimal>();
        var costoPorLote = new Dictionary<Guid, decimal>();
        var ingresoPorLote = new Dictionary<Guid, decimal>();

        void AddIngreso(CategoriaMovimiento c, decimal v) => ingresosPorCat[c] = ingresosPorCat.GetValueOrDefault(c) + v;
        void AddEgreso(CategoriaMovimiento c, decimal v) => egresosPorCat[c] = egresosPorCat.GetValueOrDefault(c) + v;
        void AddCostoLote(Guid? id, decimal v) { if (id is Guid g) costoPorLote[g] = costoPorLote.GetValueOrDefault(g) + v; }
        void AddIngresoLote(Guid? id, decimal v) { if (id is Guid g) ingresoPorLote[g] = ingresoPorLote.GetValueOrDefault(g) + v; }

        foreach (var l in lotes)
        {
            if (!InRange(l.FechaIngreso)) continue;
            decimal c = l.CantidadInicial * (l.CostoPolluelo ?? 0m);
            if (c != 0) { AddEgreso(CategoriaMovimiento.Pollitos, c); AddCostoLote(l.Id, c); }
        }
        foreach (var a in alim)
        {
            if (!InRange(a.Fecha) || a.Costo == 0) continue;
            AddEgreso(CategoriaMovimiento.Pienso, a.Costo); AddCostoLote(a.LoteId, a.Costo);
        }
        foreach (var s in san)
        {
            if (!InRange(s.Fecha) || s.Costo == 0) continue;
            AddEgreso(CategoriaMovimiento.Sanidad, s.Costo); AddCostoLote(s.LoteId, s.Costo);
        }
        foreach (var p in ped)
        {
            if (!InRange(p.Fecha) || p.Costo == 0) continue;
            var cat = MapTipoPedido(p.Tipo);
            AddEgreso(cat, p.Costo); AddCostoLote(p.LoteId, p.Costo);
        }
        foreach (var d in desp)
        {
            if (!InRange(d.Fecha) || d.Ingreso == 0) continue;
            AddIngreso(CategoriaMovimiento.VentaPollos, d.Ingreso); AddIngresoLote(d.LoteId, d.Ingreso);
        }
        foreach (var m in movs)
        {
            if (!InRange(m.Fecha)) continue;
            if (m.Tipo == TipoMovimiento.Ingreso) { AddIngreso(m.Categoria, m.Importe); AddIngresoLote(m.LoteId, m.Importe); }
            else { AddEgreso(m.Categoria, m.Importe); AddCostoLote(m.LoteId, m.Importe); }
        }

        var categorias = ingresosPorCat.Keys.Union(egresosPorCat.Keys)
            .OrderBy(c => c.ToString(), StringComparer.Ordinal)
            .Select(c => new ContabilidadCategoriaDto(
                c, decimal.Round(ingresosPorCat.GetValueOrDefault(c), 2), decimal.Round(egresosPorCat.GetValueOrDefault(c), 2)))
            .ToList();

        var porLote = lotes
            .Select(l =>
            {
                decimal costos = costoPorLote.GetValueOrDefault(l.Id);
                decimal ingresos = ingresoPorLote.GetValueOrDefault(l.Id);
                return new ContabilidadLoteDto(
                    l.Id, l.Codigo, l.Estado, decimal.Round(costos, 2), decimal.Round(ingresos, 2), decimal.Round(ingresos - costos, 2));
            })
            .OrderByDescending(x => x.Resultado)
            .ToList();

        decimal totalIngresos = ingresosPorCat.Values.Sum();
        decimal totalEgresos = egresosPorCat.Values.Sum();

        return new ContabilidadDto(
            categorias,
            porLote,
            decimal.Round(totalIngresos, 2),
            decimal.Round(totalEgresos, 2),
            decimal.Round(totalIngresos - totalEgresos, 2));
    }

    private static CategoriaMovimiento MapTipoPedido(TipoPedido tipo) => tipo switch
    {
        TipoPedido.Pienso => CategoriaMovimiento.Pienso,
        TipoPedido.Pollitos => CategoriaMovimiento.Pollitos,
        TipoPedido.Medicamento => CategoriaMovimiento.Sanidad,
        TipoPedido.Insumo => CategoriaMovimiento.Servicios,
        _ => CategoriaMovimiento.Otro,
    };
}
