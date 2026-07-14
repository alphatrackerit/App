using FSH.Framework.Shared.Persistence;
using FSH.Modules.Avicola.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Movimientos;

/// <summary>Search manual accounting movements (paged, sortable, filterable).</summary>
/// <param name="Tipo">Optional income/expense filter.</param>
/// <param name="Categoria">Optional category filter.</param>
/// <param name="LoteId">Optional flock filter.</param>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Page size (1-200).</param>
/// <param name="SortBy">Sort column. One of: fecha | importe.</param>
/// <param name="SortDir">Sort direction. One of: asc | desc.</param>
public sealed record SearchMovimientosQuery(
    TipoMovimiento? Tipo = null,
    CategoriaMovimiento? Categoria = null,
    Guid? LoteId = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null) : IQuery<PagedResponse<MovimientoDto>>;
