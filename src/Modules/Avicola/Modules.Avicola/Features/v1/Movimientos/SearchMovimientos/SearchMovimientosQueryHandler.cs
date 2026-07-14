using FSH.Framework.Shared.Persistence;
using FSH.Modules.Avicola.Contracts;
using FSH.Modules.Avicola.Contracts.Dtos;
using FSH.Modules.Avicola.Contracts.v1.Movimientos;
using FSH.Modules.Avicola.Data;
using FSH.Modules.Avicola.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Movimientos.SearchMovimientos;

public sealed class SearchMovimientosQueryHandler(AvicolaDbContext dbContext)
    : IQueryHandler<SearchMovimientosQuery, PagedResponse<MovimientoDto>>
{
    public async ValueTask<PagedResponse<MovimientoDto>> Handle(SearchMovimientosQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var q = dbContext.Movimientos.AsNoTracking().AsQueryable();

        if (query.Tipo is TipoMovimiento tipo)
        {
            q = q.Where(x => x.Tipo == tipo);
        }

        if (query.Categoria is CategoriaMovimiento categoria)
        {
            q = q.Where(x => x.Categoria == categoria);
        }

        if (query.LoteId is Guid loteId)
        {
            q = q.Where(x => x.LoteId == loteId);
        }

        q = ApplySort(q, query.SortBy, query.SortDir);

        long total = await q.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var items = await q
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResponse<MovimientoDto>
        {
            Items = items
                .Select(x => new MovimientoDto(
                    x.Id, x.Fecha, x.Tipo, x.Categoria, x.Concepto, x.Importe, x.LoteId, x.Notas))
                .ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        };
    }

    private static IQueryable<MovimientoContable> ApplySort(IQueryable<MovimientoContable> q, string? sortBy, string? sortDir)
    {
        bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        bool asc = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.ToUpperInvariant()) switch
        {
            "IMPORTE" => desc ? q.OrderByDescending(x => x.Importe) : q.OrderBy(x => x.Importe),
            _ => asc ? q.OrderBy(x => x.Fecha) : q.OrderByDescending(x => x.Fecha),
        };
    }
}
