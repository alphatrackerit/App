using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Preparaciones;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Preparaciones.UpdatePreparacion;

public static class UpdatePreparacionEndpoint
{
    internal static RouteHandlerBuilder MapUpdatePreparacionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/preparaciones/{id:guid}",
                async (Guid id, UpdatePreparacionCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command with { PreparacionId = id }, ct)))
            .WithName("UpdatePreparacion")
            .WithSummary("Update a shed preparation")
            .RequirePermission(AvicolaPermissions.Preparaciones.Update);
    }
}
