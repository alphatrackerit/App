using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Lotes;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Lotes.DeleteLote;

public static class DeleteLoteEndpoint
{
    internal static RouteHandlerBuilder MapDeleteLoteEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/lotes/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new DeleteLoteCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("DeleteLote")
            .WithSummary("Delete a flock")
            .RequirePermission(AvicolaPermissions.Lotes.Delete);
    }
}
