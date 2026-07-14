using FSH.Framework.Shared.Persistence;
using FSH.Modules.Avicola.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Preparaciones;

/// <summary>Search shed preparations (vacío sanitario), paged and filterable.</summary>
/// <param name="GalponId">Optional shed filter.</param>
/// <param name="Estado">Optional state filter (EnProceso/Completada).</param>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Page size (1-200).</param>
/// <param name="SortBy">Sort column. One of: fechaInicio | fechaFin.</param>
/// <param name="SortDir">Sort direction. One of: asc | desc.</param>
public sealed record SearchPreparacionesQuery(
    Guid? GalponId = null,
    EstadoPreparacion? Estado = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null) : IQuery<PagedResponse<PreparacionDto>>;
