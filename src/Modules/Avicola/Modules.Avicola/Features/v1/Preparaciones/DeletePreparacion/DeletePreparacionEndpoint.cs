using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Preparaciones;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Preparaciones.DeletePreparacion;

public static class DeletePreparacionEndpoint
{
    internal static RouteHandlerBuilder MapDeletePreparacionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/preparaciones/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new DeletePreparacionCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("DeletePreparacion")
            .WithSummary("Delete a shed preparation")
            .RequirePermission(AvicolaPermissions.Preparaciones.Delete);
    }
}
