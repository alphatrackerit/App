using FSH.Framework.Shared.Persistence;
using FSH.Modules.Avicola.Contracts;
using FSH.Modules.Avicola.Contracts.Dtos;
using FSH.Modules.Avicola.Contracts.v1.Lotes;
using FSH.Modules.Avicola.Data;
using FSH.Modules.Avicola.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Lotes.SearchLotes;

public sealed class SearchLotesQueryHandler(AvicolaDbContext dbContext)
    : IQueryHandler<SearchLotesQuery, PagedResponse<LoteDto>>
{
    public async ValueTask<PagedResponse<LoteDto>> Handle(SearchLotesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var q = dbContext.Lotes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string term = query.Search.Trim();
            q = q.Where(x => EF.Functions.ILike(x.Codigo, $"%{term}%"));
        }

        if (query.GalponId is Guid galponId)
        {
            q = q.Where(x => x.GalponId == galponId);
        }

        if (query.Estado is EstadoLote estado)
        {
            q = q.Where(x => x.Estado == estado);
        }

        q = ApplySort(q, query.SortBy, query.SortDir);

        long total = await q.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var items = await q
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResponse<LoteDto>
        {
            Items = items
                .Select(x => new LoteDto(
                    x.Id, x.Codigo, x.GalponId, x.Raza, x.FechaIngreso, x.CantidadInicial, x.PesoInicialGramos,
                    x.FechaSalidaPrevista, x.FechaSalidaReal, x.Estado, x.ProveedorId, x.CostoPolluelo, x.Notas))
                .ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        };
    }

    private static IQueryable<Lote> ApplySort(IQueryable<Lote> q, string? sortBy, string? sortDir)
    {
        bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        bool asc = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.ToUpperInvariant()) switch
        {
            "CODIGO" => desc ? q.OrderByDescending(x => x.Codigo) : q.OrderBy(x => x.Codigo),
            "CANTIDADINICIAL" => desc ? q.OrderByDescending(x => x.CantidadInicial) : q.OrderBy(x => x.CantidadInicial),
            // Default: newest flocks first.
            _ => asc ? q.OrderBy(x => x.FechaIngreso) : q.OrderByDescending(x => x.FechaIngreso),
        };
    }
}
