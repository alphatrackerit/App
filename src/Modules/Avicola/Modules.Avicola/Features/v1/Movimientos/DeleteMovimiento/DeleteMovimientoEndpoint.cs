using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Movimientos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Movimientos.DeleteMovimiento;

public static class DeleteMovimientoEndpoint
{
    internal static RouteHandlerBuilder MapDeleteMovimientoEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/movimientos/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new DeleteMovimientoCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("DeleteMovimiento")
            .WithSummary("Delete a manual accounting movement")
            .RequirePermission(AvicolaPermissions.Movimientos.Delete);
    }
}
