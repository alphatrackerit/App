using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Payments;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Payments.UpdatePayment;

public static class UpdatePaymentEndpoint
{
    internal static RouteHandlerBuilder MapUpdatePaymentEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/payments/{id:guid}",
                async (Guid id, UpdatePaymentCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command with { PaymentId = id }, ct)))
            .WithName("UpdatePayment")
            .WithSummary("Update a payment")
            .RequirePermission(CashflowPermissions.Payments.Update);
    }
}
