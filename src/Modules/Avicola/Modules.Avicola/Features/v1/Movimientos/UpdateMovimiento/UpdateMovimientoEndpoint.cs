using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Movimientos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Movimientos.UpdateMovimiento;

public static class UpdateMovimientoEndpoint
{
    internal static RouteHandlerBuilder MapUpdateMovimientoEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/movimientos/{id:guid}",
                async (Guid id, UpdateMovimientoCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command with { MovimientoId = id }, ct)))
            .WithName("UpdateMovimiento")
            .WithSummary("Update a manual accounting movement")
            .RequirePermission(AvicolaPermissions.Movimientos.Update);
    }
}
