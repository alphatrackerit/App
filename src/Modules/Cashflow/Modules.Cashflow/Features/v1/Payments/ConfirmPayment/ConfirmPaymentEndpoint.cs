using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Payments;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Payments.ConfirmPayment;

public static class ConfirmPaymentEndpoint
{
    internal static RouteHandlerBuilder MapConfirmPaymentEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/payments/{id:guid}/confirm",
                async (Guid id, bool? value, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new ConfirmPaymentCommand(id, value ?? true), ct);
                    return Results.NoContent();
                })
            .WithName("ConfirmPayment")
            .WithSummary("Set the confirmed flag of a payment (value=false to clear)")
            .RequirePermission(CashflowPermissions.Facturacion.ConfirmPago);
    }
}
