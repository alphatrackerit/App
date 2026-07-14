using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Galpones;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Galpones.DeleteGalpon;

public static class DeleteGalponEndpoint
{
    internal static RouteHandlerBuilder MapDeleteGalponEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/galpones/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new DeleteGalponCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("DeleteGalpon")
            .WithSummary("Delete a shed")
            .RequirePermission(AvicolaPermissions.Galpones.Delete);
    }
}
