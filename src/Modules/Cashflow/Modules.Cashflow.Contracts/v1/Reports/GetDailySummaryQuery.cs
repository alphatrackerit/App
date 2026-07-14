using FSH.Modules.Cashflow.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Reports;

/// <summary>
/// Daily cash-flow summary (spec §5.5/§6). All filters optional — omitting them = full history.
/// Dates are day-granular; <c>To</c> is inclusive (the handler treats it as &lt; To+1 day).
/// </summary>
public sealed record GetDailySummaryQuery(
    DateTime? From = null,
    DateTime? To = null,
    IReadOnlyList<Guid>? CompanyIds = null,
    IReadOnlyList<Guid>? ProjectIds = null,
    IReadOnlyList<Guid>? StatusIds = null,
    bool OnlyConfirmed = false,
    bool OnlyValidated = false) : IQuery<IReadOnlyList<DailySummaryProjectDto>>;
