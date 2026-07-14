using FSH.Framework.Shared.Persistence;
using FSH.Modules.Avicola.Contracts.Dtos;
using FSH.Modules.Avicola.Contracts.v1.Galpones;
using FSH.Modules.Avicola.Data;
using FSH.Modules.Avicola.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Galpones.SearchGalpones;

public sealed class SearchGalponesQueryHandler(AvicolaDbContext dbContext)
    : IQueryHandler<SearchGalponesQuery, PagedResponse<GalponDto>>
{
    public async ValueTask<PagedResponse<GalponDto>> Handle(SearchGalponesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var q = dbContext.Galpones.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string term = query.Search.Trim();
            q = q.Where(x => EF.Functions.ILike(x.Nombre, $"%{term}%")
                || (x.Codigo != null && EF.Functions.ILike(x.Codigo, $"%{term}%")));
        }

        if (query.Activo is bool activo)
        {
            q = q.Where(x => x.Activo == activo);
        }

        q = ApplySort(q, query.SortBy, query.SortDir);

        long total = await q.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var items = await q
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResponse<GalponDto>
        {
            Items = items
                .Select(x => new GalponDto(
                    x.Id, x.Nombre, x.Codigo, x.Capacidad, x.SuperficieM2, x.Ubicacion, x.Activo, x.Notas))
                .ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        };
    }

    private static IQueryable<Galpon> ApplySort(IQueryable<Galpon> q, string? sortBy, string? sortDir)
    {
        bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.ToUpperInvariant()) switch
        {
            "CAPACIDAD" => desc ? q.OrderByDescending(x => x.Capacidad) : q.OrderBy(x => x.Capacidad),
            _ => desc ? q.OrderByDescending(x => x.Nombre) : q.OrderBy(x => x.Nombre),
        };
    }
}
