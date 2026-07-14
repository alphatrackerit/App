using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Alimentacion;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Alimentacion.GetAlimentacionById;

public static class GetAlimentacionByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetAlimentacionByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/alimentacion/{id:guid}",
                (Guid id, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetAlimentacionByIdQuery(id), ct))
            .WithName("GetAlimentacionById")
            .WithSummary("Get a feed record by id")
            .RequirePermission(AvicolaPermissions.Alimentacion.View);
    }
}
