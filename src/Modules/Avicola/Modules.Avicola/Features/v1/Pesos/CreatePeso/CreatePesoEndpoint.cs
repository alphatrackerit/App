using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Pesos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Pesos.CreatePeso;

public static class CreatePesoEndpoint
{
    internal static RouteHandlerBuilder MapCreatePesoEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/pesos",
                async (CreatePesoCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreatePeso")
            .WithSummary("Record a weight-sampling entry")
            .RequirePermission(AvicolaPermissions.Pesos.Create)
            .WithIdempotency();
    }
}
