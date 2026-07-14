using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Pedidos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Pedidos.CambiarEstadoPedido;

public static class CambiarEstadoPedidoEndpoint
{
    internal static RouteHandlerBuilder MapCambiarEstadoPedidoEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/pedidos/{id:guid}/estado",
                async (Guid id, CambiarEstadoPedidoCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command with { PedidoId = id }, ct)))
            .WithName("CambiarEstadoPedido")
            .WithSummary("Apply a lifecycle transition to an order (enviar/recibir/cancelar)")
            .RequirePermission(AvicolaPermissions.Pedidos.Update);
    }
}
