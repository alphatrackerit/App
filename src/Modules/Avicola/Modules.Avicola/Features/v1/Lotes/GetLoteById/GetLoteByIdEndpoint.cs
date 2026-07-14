using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Lotes;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Lotes.GetLoteById;

public static class GetLoteByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetLoteByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/lotes/{id:guid}",
                (Guid id, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetLoteByIdQuery(id), ct))
            .WithName("GetLoteById")
            .WithSummary("Get a flock by id")
            .RequirePermission(AvicolaPermissions.Lotes.View);
    }
}
