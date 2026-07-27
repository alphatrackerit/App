using FSH.Framework.Shared.Persistence;
using FSH.Modules.Cashflow.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Projects;

/// <summary>
/// Search for projects with pagination and sorting.
/// </summary>
/// <param name="Search">Free-text term matched against the project name and the names of its client, country, status and company.</param>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Page size (1-200).</param>
/// <param name="SortBy">Sort column. One of: name | salePrice | cost | profit.</param>
/// <param name="SortDir">Sort direction. One of: asc | desc.</param>
/// <param name="CompanyId">When set, only projects belonging to this company.</param>
public sealed record SearchProjectsQuery(
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null,
    Guid? CompanyId = null) : IQuery<PagedResponse<ProjectDto>>;
