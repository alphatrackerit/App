using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Pesos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Pesos.UpdatePeso;

public static class UpdatePesoEndpoint
{
    internal static RouteHandlerBuilder MapUpdatePesoEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/pesos/{id:guid}",
                async (Guid id, UpdatePesoCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command with { PesoId = id }, ct)))
            .WithName("UpdatePeso")
            .WithSummary("Update a weight record")
            .RequirePermission(AvicolaPermissions.Pesos.Update);
    }
}
