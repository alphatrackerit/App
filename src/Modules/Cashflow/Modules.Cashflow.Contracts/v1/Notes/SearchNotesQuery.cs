using FSH.Framework.Shared.Persistence;
using FSH.Modules.Cashflow.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Notes;

/// <summary>
/// Search for notes with pagination, sorting and an optional project filter.
/// </summary>
/// <param name="Search">Free-text term matched against the title / description.</param>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Page size (1-200).</param>
/// <param name="SortBy">Sort column. One of: title | date.</param>
/// <param name="SortDir">Sort direction. One of: asc | desc.</param>
/// <param name="ProjectId">Optional filter — only notes belonging to this project.</param>
public sealed record SearchNotesQuery(
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null,
    Guid? ProjectId = null) : IQuery<PagedResponse<NoteDto>>;
