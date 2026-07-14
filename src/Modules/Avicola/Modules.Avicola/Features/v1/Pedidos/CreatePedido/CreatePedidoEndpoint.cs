using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Pedidos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Pedidos.CreatePedido;

public static class CreatePedidoEndpoint
{
    internal static RouteHandlerBuilder MapCreatePedidoEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/pedidos",
                async (CreatePedidoCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreatePedido")
            .WithSummary("Create a purchase order")
            .RequirePermission(AvicolaPermissions.Pedidos.Create)
            .WithIdempotency();
    }
}
