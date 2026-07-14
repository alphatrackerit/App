using FSH.Framework.Shared.Persistence;
using FSH.Modules.Avicola.Contracts;
using FSH.Modules.Avicola.Contracts.Dtos;
using FSH.Modules.Avicola.Contracts.v1.Alimentacion;
using FSH.Modules.Avicola.Data;
using FSH.Modules.Avicola.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Alimentacion.SearchAlimentacion;

public sealed class SearchAlimentacionQueryHandler(AvicolaDbContext dbContext)
    : IQueryHandler<SearchAlimentacionQuery, PagedResponse<AlimentacionDto>>
{
    public async ValueTask<PagedResponse<AlimentacionDto>> Handle(SearchAlimentacionQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var q = dbContext.Alimentaciones.AsNoTracking().AsQueryable();

        if (query.LoteId is Guid loteId)
        {
            q = q.Where(x => x.LoteId == loteId);
        }

        if (query.TipoAlimento is TipoAlimento tipo)
        {
            q = q.Where(x => x.TipoAlimento == tipo);
        }

        q = ApplySort(q, query.SortBy, query.SortDir);

        long total = await q.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var items = await q
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResponse<AlimentacionDto>
        {
            Items = items
                .Select(x => new AlimentacionDto(
                    x.Id, x.LoteId, x.Fecha, x.TipoAlimento, x.CantidadKg, x.CostoUnitario, x.Notas))
                .ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        };
    }

    private static IQueryable<RegistroAlimentacion> ApplySort(IQueryable<RegistroAlimentacion> q, string? sortBy, string? sortDir)
    {
        bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        bool asc = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.ToUpperInvariant()) switch
        {
            "CANTIDADKG" => desc ? q.OrderByDescending(x => x.CantidadKg) : q.OrderBy(x => x.CantidadKg),
            _ => asc ? q.OrderBy(x => x.Fecha) : q.OrderByDescending(x => x.Fecha),
        };
    }
}
