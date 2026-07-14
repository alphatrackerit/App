using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Preparaciones;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Preparaciones.CreatePreparacion;

public static class CreatePreparacionEndpoint
{
    internal static RouteHandlerBuilder MapCreatePreparacionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/preparaciones",
                async (CreatePreparacionCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreatePreparacion")
            .WithSummary("Start a shed preparation (vacío sanitario) for the next flock")
            .RequirePermission(AvicolaPermissions.Preparaciones.Create)
            .WithIdempotency();
    }
}
