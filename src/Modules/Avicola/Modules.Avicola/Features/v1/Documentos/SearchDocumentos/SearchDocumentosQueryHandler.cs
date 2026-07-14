using FSH.Framework.Shared.Persistence;
using FSH.Modules.Avicola.Contracts;
using FSH.Modules.Avicola.Contracts.Dtos;
using FSH.Modules.Avicola.Contracts.v1.Documentos;
using FSH.Modules.Avicola.Data;
using FSH.Modules.Avicola.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Documentos.SearchDocumentos;

public sealed class SearchDocumentosQueryHandler(AvicolaDbContext dbContext)
    : IQueryHandler<SearchDocumentosQuery, PagedResponse<DocumentoDto>>
{
    public async ValueTask<PagedResponse<DocumentoDto>> Handle(SearchDocumentosQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var q = dbContext.Documentos.AsNoTracking().AsQueryable();

        if (query.Origen is DocumentoOrigen origen)
        {
            q = q.Where(x => x.Origen == origen);
        }

        if (query.OrigenId is Guid origenId)
        {
            q = q.Where(x => x.OrigenId == origenId);
        }

        if (query.Tipo is TipoDocumento tipo)
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

        return new PagedResponse<DocumentoDto>
        {
            Items = items
                .Select(x => new DocumentoDto(
                    x.Id, x.Tipo, x.Origen, x.OrigenId, x.FileAssetId, x.Url, x.NombreArchivo, x.ContentType, x.Fecha, x.Notas))
                .ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        };
    }

    private static IQueryable<Documento> ApplySort(IQueryable<Documento> q, string? sortBy, string? sortDir)
    {
        bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        bool asc = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.ToUpperInvariant()) switch
        {
            "NOMBRE" => desc ? q.OrderByDescending(x => x.NombreArchivo) : q.OrderBy(x => x.NombreArchivo),
            _ => asc ? q.OrderBy(x => x.Fecha) : q.OrderByDescending(x => x.Fecha),
        };
    }
}
