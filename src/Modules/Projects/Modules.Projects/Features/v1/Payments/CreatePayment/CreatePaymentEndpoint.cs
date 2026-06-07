using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Projects.Contracts.Authorization;
using FSH.Modules.Projects.Contracts.v1.Payments;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Projects.Features.v1.Payments.CreatePayment;

public static class CreatePaymentEndpoint
{
    internal static RouteHandlerBuilder MapCreatePaymentEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/payments",
                async (CreatePaymentCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreatePayment")
            .WithSummary("Create a payment")
            .RequirePermission(ProjectsPermissions.Payments.Create)
            .WithIdempotency();
    }
}
