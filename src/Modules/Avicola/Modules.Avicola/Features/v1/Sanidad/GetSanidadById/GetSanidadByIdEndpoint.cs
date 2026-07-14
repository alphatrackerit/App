using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Sanidad;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Sanidad.GetSanidadById;

public static class GetSanidadByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetSanidadByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/sanidad/{id:guid}",
                (Guid id, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetSanidadByIdQuery(id), ct))
            .WithName("GetSanidadById")
            .WithSummary("Get a health record by id")
            .RequirePermission(AvicolaPermissions.Sanidad.View);
    }
}
