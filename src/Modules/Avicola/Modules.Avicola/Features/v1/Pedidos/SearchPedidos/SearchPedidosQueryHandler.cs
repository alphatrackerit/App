using FSH.Framework.Shared.Persistence;
using FSH.Modules.Avicola.Contracts;
using FSH.Modules.Avicola.Contracts.Dtos;
using FSH.Modules.Avicola.Contracts.v1.Pedidos;
using FSH.Modules.Avicola.Data;
using FSH.Modules.Avicola.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Pedidos.SearchPedidos;

public sealed class SearchPedidosQueryHandler(AvicolaDbContext dbContext)
    : IQueryHandler<SearchPedidosQuery, PagedResponse<PedidoDto>>
{
    public async ValueTask<PagedResponse<PedidoDto>> Handle(SearchPedidosQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var q = dbContext.Pedidos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string term = query.Search.Trim();
            q = q.Where(x => EF.Functions.ILike(x.Codigo, $"%{term}%"));
        }

        if (query.Tipo is TipoPedido tipo)
        {
            q = q.Where(x => x.Tipo == tipo);
        }

        if (query.Estado is EstadoPedido estado)
        {
            q = q.Where(x => x.Estado == estado);
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

        return new PagedResponse<PedidoDto>
        {
            Items = items
                .Select(x => new PedidoDto(
                    x.Id, x.Codigo, x.Tipo, x.ProveedorId, x.Descripcion, x.Cantidad, x.Unidad, x.CostoEstimado,
                    x.CostoReal, x.Estado, x.FechaPedido, x.FechaRecepcion, x.LoteId, x.GalponId, x.Notas))
                .ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        };
    }

    private static IQueryable<Pedido> ApplySort(IQueryable<Pedido> q, string? sortBy, string? sortDir)
    {
        bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        bool asc = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.ToUpperInvariant()) switch
        {
            "CODIGO" => desc ? q.OrderByDescending(x => x.Codigo) : q.OrderBy(x => x.Codigo),
            "COSTOESTIMADO" => desc ? q.OrderByDescending(x => x.CostoEstimado) : q.OrderBy(x => x.CostoEstimado),
            _ => asc ? q.OrderBy(x => x.FechaPedido) : q.OrderByDescending(x => x.FechaPedido),
        };
    }
}
