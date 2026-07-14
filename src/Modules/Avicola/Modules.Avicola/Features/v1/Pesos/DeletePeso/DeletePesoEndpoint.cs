using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Pesos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Pesos.DeletePeso;

public static class DeletePesoEndpoint
{
    internal static RouteHandlerBuilder MapDeletePesoEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/pesos/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new DeletePesoCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("DeletePeso")
            .WithSummary("Delete a weight record")
            .RequirePermission(AvicolaPermissions.Pesos.Delete);
    }
}
