using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Payments;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Payments.ValidatePayment;

public static class ValidatePaymentEndpoint
{
    internal static RouteHandlerBuilder MapValidatePaymentEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/payments/{id:guid}/validate",
                async (Guid id, bool? value, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new ValidatePaymentCommand(id, value ?? true), ct);
                    return Results.NoContent();
                })
            .WithName("ValidatePayment")
            .WithSummary("Set the validated flag of a payment (value=false to clear)")
            .RequirePermission(CashflowPermissions.Facturacion.ValidatePago);
    }
}
