using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Galpones;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Galpones.UpdateGalpon;

public static class UpdateGalponEndpoint
{
    internal static RouteHandlerBuilder MapUpdateGalponEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/galpones/{id:guid}",
                async (Guid id, UpdateGalponCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command with { GalponId = id }, ct)))
            .WithName("UpdateGalpon")
            .WithSummary("Update a shed")
            .RequirePermission(AvicolaPermissions.Galpones.Update);
    }
}
