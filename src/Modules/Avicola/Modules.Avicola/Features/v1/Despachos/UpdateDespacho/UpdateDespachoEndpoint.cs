using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Despachos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Despachos.UpdateDespacho;

public static class UpdateDespachoEndpoint
{
    internal static RouteHandlerBuilder MapUpdateDespachoEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/despachos/{id:guid}",
                async (Guid id, UpdateDespachoCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command with { DespachoId = id }, ct)))
            .WithName("UpdateDespacho")
            .WithSummary("Update a dispatch record")
            .RequirePermission(AvicolaPermissions.Despachos.Update);
    }
}
