using FSH.Framework.Shared.Persistence;
using FSH.Modules.Avicola.Contracts.Dtos;
using FSH.Modules.Avicola.Contracts.v1.Pesos;
using FSH.Modules.Avicola.Data;
using FSH.Modules.Avicola.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Pesos.SearchPesos;

public sealed class SearchPesosQueryHandler(AvicolaDbContext dbContext)
    : IQueryHandler<SearchPesosQuery, PagedResponse<PesoDto>>
{
    public async ValueTask<PagedResponse<PesoDto>> Handle(SearchPesosQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var q = dbContext.Pesos.AsNoTracking().AsQueryable();

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

        return new PagedResponse<PesoDto>
        {
            Items = items
                .Select(x => new PesoDto(
                    x.Id, x.LoteId, x.Fecha, x.PesoPromedioGramos, x.CantidadMuestra, x.Notas))
                .ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        };
    }

    private static IQueryable<RegistroPeso> ApplySort(IQueryable<RegistroPeso> q, string? sortBy, string? sortDir)
    {
        bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        bool asc = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.ToUpperInvariant()) switch
        {
            "PESOPROMEDIOGRAMOS" => desc ? q.OrderByDescending(x => x.PesoPromedioGramos) : q.OrderBy(x => x.PesoPromedioGramos),
            _ => asc ? q.OrderBy(x => x.Fecha) : q.OrderByDescending(x => x.Fecha),
        };
    }
}
