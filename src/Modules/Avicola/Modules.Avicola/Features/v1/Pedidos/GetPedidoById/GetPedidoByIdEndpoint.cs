using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Pedidos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Pedidos.GetPedidoById;

public static class GetPedidoByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetPedidoByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/pedidos/{id:guid}",
                (Guid id, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetPedidoByIdQuery(id), ct))
            .WithName("GetPedidoById")
            .WithSummary("Get a purchase order by id")
            .RequirePermission(AvicolaPermissions.Pedidos.View);
    }
}
