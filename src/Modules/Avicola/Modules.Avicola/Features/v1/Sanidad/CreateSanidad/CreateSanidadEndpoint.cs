using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Sanidad;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Sanidad.CreateSanidad;

public static class CreateSanidadEndpoint
{
    internal static RouteHandlerBuilder MapCreateSanidadEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/sanidad",
                async (CreateSanidadCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateSanidad")
            .WithSummary("Record a health entry (vaccination / medication / treatment)")
            .RequirePermission(AvicolaPermissions.Sanidad.Create)
            .WithIdempotency();
    }
}
