using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.GetProformaById;

public static class GetProformaByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetProformaByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/proformas/{id:guid}",
                (Guid id, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetProformaByIdQuery(id), ct))
            .WithName("GetProformaById")
            .WithSummary("Get a proforma by id")
            .RequirePermission(CashflowPermissions.Proformas.View);
    }
}
