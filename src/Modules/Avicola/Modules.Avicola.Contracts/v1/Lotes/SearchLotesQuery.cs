using FSH.Framework.Shared.Persistence;
using FSH.Modules.Avicola.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Lotes;

/// <summary>Search flocks with pagination, sorting and filters.</summary>
/// <param name="Search">Free-text term matched against the flock code.</param>
/// <param name="GalponId">Optional shed filter.</param>
/// <param name="Estado">Optional lifecycle-state filter.</param>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Page size (1-200).</param>
/// <param name="SortBy">Sort column. One of: codigo | fechaIngreso | cantidadInicial.</param>
/// <param name="SortDir">Sort direction. One of: asc | desc.</param>
public sealed record SearchLotesQuery(
    string? Search = null,
    Guid? GalponId = null,
    EstadoLote? Estado = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null) : IQuery<PagedResponse<LoteDto>>;
