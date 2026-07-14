using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Reports;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Reports.GetDailySummary;

public static class GetDailySummaryEndpoint
{
    internal static RouteHandlerBuilder MapGetDailySummaryEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/reports/daily-summary",
            (DateTime? from, DateTime? to, Guid[]? companyIds, Guid[]? projectIds, Guid[]? statusIds,
             bool? onlyConfirmed, bool? onlyValidated, IMediator mediator, CancellationToken ct) =>
                mediator.Send(new GetDailySummaryQuery(
                    from, to, companyIds, projectIds, statusIds,
                    onlyConfirmed ?? false, onlyValidated ?? false), ct))
            .WithName("GetDailySummary")
            .WithSummary("Daily cash-flow summary per project (spec §6)")
            .RequirePermission(CashflowPermissions.Reportes.View);
}
