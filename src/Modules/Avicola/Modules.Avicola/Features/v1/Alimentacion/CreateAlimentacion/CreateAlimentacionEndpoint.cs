using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Alimentacion;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Alimentacion.CreateAlimentacion;

public static class CreateAlimentacionEndpoint
{
    internal static RouteHandlerBuilder MapCreateAlimentacionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/alimentacion",
                async (CreateAlimentacionCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateAlimentacion")
            .WithSummary("Record a feed-consumption entry")
            .RequirePermission(AvicolaPermissions.Alimentacion.Create)
            .WithIdempotency();
    }
}
