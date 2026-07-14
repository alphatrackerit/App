using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Despachos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Despachos.CreateDespacho;

public static class CreateDespachoEndpoint
{
    internal static RouteHandlerBuilder MapCreateDespachoEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/despachos",
                async (CreateDespachoCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateDespacho")
            .WithSummary("Record a harvest / dispatch entry")
            .RequirePermission(AvicolaPermissions.Despachos.Create)
            .WithIdempotency();
    }
}
