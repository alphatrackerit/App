using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Sanidad;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Sanidad.DeleteSanidad;

public static class DeleteSanidadEndpoint
{
    internal static RouteHandlerBuilder MapDeleteSanidadEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/sanidad/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new DeleteSanidadCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("DeleteSanidad")
            .WithSummary("Delete a health record")
            .RequirePermission(AvicolaPermissions.Sanidad.Delete);
    }
}
