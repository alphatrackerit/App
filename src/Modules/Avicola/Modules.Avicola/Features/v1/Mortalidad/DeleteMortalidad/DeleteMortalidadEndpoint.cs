using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Mortalidad;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Mortalidad.DeleteMortalidad;

public static class DeleteMortalidadEndpoint
{
    internal static RouteHandlerBuilder MapDeleteMortalidadEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/mortalidad/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new DeleteMortalidadCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("DeleteMortalidad")
            .WithSummary("Delete a mortality record")
            .RequirePermission(AvicolaPermissions.Mortalidad.Delete);
    }
}
