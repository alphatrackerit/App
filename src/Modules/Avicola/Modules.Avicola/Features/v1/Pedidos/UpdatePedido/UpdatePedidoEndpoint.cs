using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Pedidos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Pedidos.UpdatePedido;

public static class UpdatePedidoEndpoint
{
    internal static RouteHandlerBuilder MapUpdatePedidoEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/pedidos/{id:guid}",
                async (Guid id, UpdatePedidoCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command with { PedidoId = id }, ct)))
            .WithName("UpdatePedido")
            .WithSummary("Update a purchase order")
            .RequirePermission(AvicolaPermissions.Pedidos.Update);
    }
}
