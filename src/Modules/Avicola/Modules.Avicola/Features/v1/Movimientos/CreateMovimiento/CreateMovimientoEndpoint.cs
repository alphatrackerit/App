using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Movimientos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Movimientos.CreateMovimiento;

public static class CreateMovimientoEndpoint
{
    internal static RouteHandlerBuilder MapCreateMovimientoEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/movimientos",
                async (CreateMovimientoCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateMovimiento")
            .WithSummary("Record a manual accounting movement")
            .RequirePermission(AvicolaPermissions.Movimientos.Create)
            .WithIdempotency();
    }
}
