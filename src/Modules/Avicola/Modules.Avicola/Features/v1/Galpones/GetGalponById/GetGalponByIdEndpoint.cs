using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Galpones;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Galpones.GetGalponById;

public static class GetGalponByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetGalponByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/galpones/{id:guid}",
                (Guid id, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetGalponByIdQuery(id), ct))
            .WithName("GetGalponById")
            .WithSummary("Get a shed by id")
            .RequirePermission(AvicolaPermissions.Galpones.View);
    }
}
