using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.GetProformaLines;

public static class GetProformaLinesEndpoint
{
    internal static RouteHandlerBuilder MapGetProformaLinesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/proformas/{id:guid}/lines",
                (Guid id, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetProformaLinesQuery(id), ct))
            .WithName("GetProformaLines")
            .WithSummary("Get a proforma's linked invoices and informative totals")
            .RequirePermission(CashflowPermissions.Proformas.View);
    }
}
