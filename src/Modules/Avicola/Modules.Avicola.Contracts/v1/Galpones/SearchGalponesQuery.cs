using FSH.Framework.Shared.Persistence;
using FSH.Modules.Avicola.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Galpones;

/// <summary>Search sheds with pagination and sorting.</summary>
/// <param name="Search">Free-text term matched against name / code.</param>
/// <param name="Activo">Optional active-state filter.</param>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Page size (1-200).</param>
/// <param name="SortBy">Sort column. One of: nombre | capacidad.</param>
/// <param name="SortDir">Sort direction. One of: asc | desc.</param>
public sealed record SearchGalponesQuery(
    string? Search = null,
    bool? Activo = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null) : IQuery<PagedResponse<GalponDto>>;
