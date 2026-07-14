using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Mortalidad;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Mortalidad.CreateMortalidad;

public static class CreateMortalidadEndpoint
{
    internal static RouteHandlerBuilder MapCreateMortalidadEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/mortalidad",
                async (CreateMortalidadCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateMortalidad")
            .WithSummary("Record a mortality / cull entry")
            .RequirePermission(AvicolaPermissions.Mortalidad.Create)
            .WithIdempotency();
    }
}
