using FSH.Framework.Shared.Persistence;
using FSH.Modules.Avicola.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Pedidos;

/// <summary>Search purchase orders (paged, sortable, filterable).</summary>
/// <param name="Search">Free-text term matched against the order code.</param>
/// <param name="Tipo">Optional order-type filter.</param>
/// <param name="Estado">Optional state filter.</param>
/// <param name="LoteId">Optional flock filter.</param>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Page size (1-200).</param>
/// <param name="SortBy">Sort column. One of: codigo | fechaPedido | costoEstimado.</param>
/// <param name="SortDir">Sort direction. One of: asc | desc.</param>
public sealed record SearchPedidosQuery(
    string? Search = null,
    TipoPedido? Tipo = null,
    EstadoPedido? Estado = null,
    Guid? LoteId = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null) : IQuery<PagedResponse<PedidoDto>>;
