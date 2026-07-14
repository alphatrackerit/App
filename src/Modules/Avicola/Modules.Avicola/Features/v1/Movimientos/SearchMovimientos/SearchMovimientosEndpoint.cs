using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Movimientos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Movimientos.SearchMovimientos;

public static class SearchMovimientosEndpoint
{
    internal static RouteHandlerBuilder MapSearchMovimientosEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/movimientos",
                (TipoMovimiento? tipo, CategoriaMovimiento? categoria, Guid? loteId, int? pageNumber, int? pageSize,
                 string? sortBy, string? sortDir, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(
                        new SearchMovimientosQuery(tipo, categoria, loteId, pageNumber ?? 1, pageSize ?? 20, sortBy, sortDir),
                        ct))
            .WithName("SearchMovimientos")
            .WithSummary("Search manual accounting movements (paged, filterable)")
            .RequirePermission(AvicolaPermissions.Movimientos.View);
    }
}
