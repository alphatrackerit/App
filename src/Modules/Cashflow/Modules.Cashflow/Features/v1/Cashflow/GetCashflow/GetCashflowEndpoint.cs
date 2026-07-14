using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Cashflow;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Cashflow.GetCashflow;

public static class GetCashflowEndpoint
{
    internal static RouteHandlerBuilder MapGetCashflowEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/cashflow",
                (IMediator mediator, CancellationToken ct) => mediator.Send(new GetCashflowQuery(), ct))
            .WithName("GetCashflow")
            .WithSummary("Cash-flow ledger: every dated income and payment plus the project list")
            .RequirePermission(CashflowPermissions.Projects.View);
    }
}
