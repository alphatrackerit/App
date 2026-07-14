using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Galpones;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Galpones.CreateGalpon;

public static class CreateGalponEndpoint
{
    internal static RouteHandlerBuilder MapCreateGalponEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/galpones",
                async (CreateGalponCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateGalpon")
            .WithSummary("Create a shed (galpón)")
            .RequirePermission(AvicolaPermissions.Galpones.Create)
            .WithIdempotency();
    }
}
