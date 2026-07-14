using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Despachos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Despachos.GetDespachoById;

public static class GetDespachoByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetDespachoByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/despachos/{id:guid}",
                (Guid id, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetDespachoByIdQuery(id), ct))
            .WithName("GetDespachoById")
            .WithSummary("Get a dispatch record by id")
            .RequirePermission(AvicolaPermissions.Despachos.View);
    }
}
