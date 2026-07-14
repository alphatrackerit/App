using FSH.Framework.Shared.Persistence;
using FSH.Modules.Avicola.Contracts.Dtos;
using FSH.Modules.Avicola.Contracts.v1.Despachos;
using FSH.Modules.Avicola.Data;
using FSH.Modules.Avicola.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Despachos.SearchDespachos;

public sealed class SearchDespachosQueryHandler(AvicolaDbContext dbContext)
    : IQueryHandler<SearchDespachosQuery, PagedResponse<DespachoDto>>
{
    public async ValueTask<PagedResponse<DespachoDto>> Handle(SearchDespachosQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var q = dbContext.Despachos.AsNoTracking().AsQueryable();

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

        return new PagedResponse<DespachoDto>
        {
            Items = items
                .Select(x => new DespachoDto(
                    x.Id, x.LoteId, x.Fecha, x.Cantidad, x.PesoTotalKg, x.PrecioPorKg, x.ClienteId, x.Notas))
                .ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        };
    }

    private static IQueryable<Despacho> ApplySort(IQueryable<Despacho> q, string? sortBy, string? sortDir)
    {
        bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        bool asc = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.ToUpperInvariant()) switch
        {
            "CANTIDAD" => desc ? q.OrderByDescending(x => x.Cantidad) : q.OrderBy(x => x.Cantidad),
            "PESOTOTALKG" => desc ? q.OrderByDescending(x => x.PesoTotalKg) : q.OrderBy(x => x.PesoTotalKg),
            _ => asc ? q.OrderBy(x => x.Fecha) : q.OrderByDescending(x => x.Fecha),
        };
    }
}
