using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Despachos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Despachos.DeleteDespacho;

public static class DeleteDespachoEndpoint
{
    internal static RouteHandlerBuilder MapDeleteDespachoEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/despachos/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new DeleteDespachoCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("DeleteDespacho")
            .WithSummary("Delete a dispatch record")
            .RequirePermission(AvicolaPermissions.Despachos.Delete);
    }
}
