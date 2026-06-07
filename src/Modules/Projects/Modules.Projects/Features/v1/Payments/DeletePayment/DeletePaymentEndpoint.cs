using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Projects.Contracts.Authorization;
using FSH.Modules.Projects.Contracts.v1.Payments;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Projects.Features.v1.Payments.DeletePayment;

public static class DeletePaymentEndpoint
{
    internal static RouteHandlerBuilder MapDeletePaymentEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/payments/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new DeletePaymentCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("DeletePayment")
            .WithSummary("Delete a payment")
            .RequirePermission(ProjectsPermissions.Payments.Delete);
    }
}
