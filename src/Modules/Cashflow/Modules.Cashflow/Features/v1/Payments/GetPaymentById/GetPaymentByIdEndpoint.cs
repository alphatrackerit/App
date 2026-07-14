using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Payments;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Payments.GetPaymentById;

public static class GetPaymentByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetPaymentByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/payments/{id:guid}",
                (Guid id, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetPaymentByIdQuery(id), ct))
            .WithName("GetPaymentById")
            .WithSummary("Get a payment by id")
            .RequirePermission(CashflowPermissions.Payments.View);
    }
}
