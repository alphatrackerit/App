using FSH.Framework.Shared.Persistence;
using FSH.Modules.Avicola.Contracts;
using FSH.Modules.Avicola.Contracts.Dtos;
using FSH.Modules.Avicola.Contracts.v1.Preparaciones;
using FSH.Modules.Avicola.Data;
using FSH.Modules.Avicola.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Preparaciones.SearchPreparaciones;

public sealed class SearchPreparacionesQueryHandler(AvicolaDbContext dbContext)
    : IQueryHandler<SearchPreparacionesQuery, PagedResponse<PreparacionDto>>
{
    public async ValueTask<PagedResponse<PreparacionDto>> Handle(SearchPreparacionesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var q = dbContext.Preparaciones.AsNoTracking().AsQueryable();

        if (query.GalponId is Guid galponId)
        {
            q = q.Where(x => x.GalponId == galponId);
        }

        if (query.Estado is EstadoPreparacion estado)
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

        return new PagedResponse<PreparacionDto>
        {
            Items = items
                .Select(x => new PreparacionDto(
                    x.Id, x.GalponId, x.LoteAnteriorId, x.FechaRetiro, x.FechaInicio, x.FechaFin,
                    x.RetiradaCama, x.Lavado, x.Desinfeccion, x.Desinsectacion, x.CamaNueva, x.Costo, x.Estado, x.Notas))
                .ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        };
    }

    private static IQueryable<PreparacionNave> ApplySort(IQueryable<PreparacionNave> q, string? sortBy, string? sortDir)
    {
        bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        bool asc = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.ToUpperInvariant()) switch
        {
            "FECHAFIN" => desc ? q.OrderByDescending(x => x.FechaFin) : q.OrderBy(x => x.FechaFin),
            _ => asc ? q.OrderBy(x => x.FechaInicio) : q.OrderByDescending(x => x.FechaInicio),
        };
    }
}
