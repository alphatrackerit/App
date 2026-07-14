using FSH.Framework.Shared.Persistence;
using FSH.Modules.Avicola.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Pesos;

/// <summary>Search weight records (paged, sortable), optionally scoped to one flock.</summary>
/// <param name="LoteId">Optional flock filter.</param>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Page size (1-200).</param>
/// <param name="SortBy">Sort column. One of: fecha | pesoPromedioGramos.</param>
/// <param name="SortDir">Sort direction. One of: asc | desc.</param>
public sealed record SearchPesosQuery(
    Guid? LoteId = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null) : IQuery<PagedResponse<PesoDto>>;
