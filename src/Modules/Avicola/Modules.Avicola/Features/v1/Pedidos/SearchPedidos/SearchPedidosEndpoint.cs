using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Pedidos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Pedidos.SearchPedidos;

public static class SearchPedidosEndpoint
{
    internal static RouteHandlerBuilder MapSearchPedidosEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/pedidos",
                (string? search, TipoPedido? tipo, EstadoPedido? estado, Guid? loteId, int? pageNumber, int? pageSize,
                 string? sortBy, string? sortDir, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(
                        new SearchPedidosQuery(search, tipo, estado, loteId, pageNumber ?? 1, pageSize ?? 20, sortBy, sortDir),
                        ct))
            .WithName("SearchPedidos")
            .WithSummary("Search purchase orders (paged, filterable)")
            .RequirePermission(AvicolaPermissions.Pedidos.View);
    }
}
