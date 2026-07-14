using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Preparaciones;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Preparaciones.GetPreparacionById;

public static class GetPreparacionByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetPreparacionByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/preparaciones/{id:guid}",
                (Guid id, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetPreparacionByIdQuery(id), ct))
            .WithName("GetPreparacionById")
            .WithSummary("Get a shed preparation by id")
            .RequirePermission(AvicolaPermissions.Preparaciones.View);
    }
}
