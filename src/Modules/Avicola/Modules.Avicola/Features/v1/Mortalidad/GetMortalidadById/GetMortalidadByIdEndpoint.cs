using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Mortalidad;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Mortalidad.GetMortalidadById;

public static class GetMortalidadByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetMortalidadByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/mortalidad/{id:guid}",
                (Guid id, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetMortalidadByIdQuery(id), ct))
            .WithName("GetMortalidadById")
            .WithSummary("Get a mortality record by id")
            .RequirePermission(AvicolaPermissions.Mortalidad.View);
    }
}
