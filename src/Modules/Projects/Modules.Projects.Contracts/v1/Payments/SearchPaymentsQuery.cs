using FSH.Framework.Shared.Persistence;
using FSH.Modules.Projects.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Projects.Contracts.v1.Payments;

/// <summary>
/// Search for payments with pagination, sorting and an optional project filter.
/// </summary>
/// <param name="Search">Free-text term matched against the description.</param>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Page size (1-200).</param>
/// <param name="SortBy">Sort column. One of: amount | date | percentage.</param>
/// <param name="SortDir">Sort direction. One of: asc | desc.</param>
/// <param name="ProjectId">Optional filter — only payments belonging to this project.</param>
public sealed record SearchPaymentsQuery(
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null,
    Guid? ProjectId = null) : IQuery<PagedResponse<PaymentDto>>;
