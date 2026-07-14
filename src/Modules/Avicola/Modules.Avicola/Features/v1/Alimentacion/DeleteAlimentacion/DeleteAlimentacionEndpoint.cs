using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Alimentacion;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Alimentacion.DeleteAlimentacion;

public static class DeleteAlimentacionEndpoint
{
    internal static RouteHandlerBuilder MapDeleteAlimentacionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/alimentacion/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new DeleteAlimentacionCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("DeleteAlimentacion")
            .WithSummary("Delete a feed record")
            .RequirePermission(AvicolaPermissions.Alimentacion.Delete);
    }
}
