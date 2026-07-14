using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Pesos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Pesos.GetPesoById;

public static class GetPesoByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetPesoByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/pesos/{id:guid}",
                (Guid id, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetPesoByIdQuery(id), ct))
            .WithName("GetPesoById")
            .WithSummary("Get a weight record by id")
            .RequirePermission(AvicolaPermissions.Pesos.View);
    }
}
