using FSH.Framework.Shared.Persistence;
using FSH.Modules.Avicola.Contracts;
using FSH.Modules.Avicola.Contracts.Dtos;
using FSH.Modules.Avicola.Contracts.v1.Sanidad;
using FSH.Modules.Avicola.Data;
using FSH.Modules.Avicola.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Sanidad.SearchSanidad;

public sealed class SearchSanidadQueryHandler(AvicolaDbContext dbContext)
    : IQueryHandler<SearchSanidadQuery, PagedResponse<SanidadDto>>
{
    public async ValueTask<PagedResponse<SanidadDto>> Handle(SearchSanidadQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var q = dbContext.RegistrosSanitarios.AsNoTracking().AsQueryable();

        if (query.LoteId is Guid loteId)
        {
            q = q.Where(x => x.LoteId == loteId);
        }

        if (query.Tipo is TipoRegistroSanitario tipo)
        {
            q = q.Where(x => x.Tipo == tipo);
        }

        q = ApplySort(q, query.SortBy, query.SortDir);

        long total = await q.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var items = await q
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResponse<SanidadDto>
        {
            Items = items
                .Select(x => new SanidadDto(
                    x.Id, x.LoteId, x.Fecha, x.Tipo, x.Producto, x.Dosis, x.ViaAplicacion, x.Costo, x.Notas))
                .ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        };
    }

    private static IQueryable<RegistroSanitario> ApplySort(IQueryable<RegistroSanitario> q, string? sortBy, string? sortDir)
    {
        bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        bool asc = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.ToUpperInvariant()) switch
        {
            "PRODUCTO" => desc ? q.OrderByDescending(x => x.Producto) : q.OrderBy(x => x.Producto),
            _ => asc ? q.OrderBy(x => x.Fecha) : q.OrderByDescending(x => x.Fecha),
        };
    }
}
