using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Pedidos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Pedidos.DeletePedido;

public static class DeletePedidoEndpoint
{
    internal static RouteHandlerBuilder MapDeletePedidoEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/pedidos/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new DeletePedidoCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("DeletePedido")
            .WithSummary("Delete a purchase order")
            .RequirePermission(AvicolaPermissions.Pedidos.Delete);
    }
}
