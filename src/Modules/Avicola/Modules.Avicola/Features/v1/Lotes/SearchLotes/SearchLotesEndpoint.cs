using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Lotes;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Lotes.SearchLotes;

public static class SearchLotesEndpoint
{
    internal static RouteHandlerBuilder MapSearchLotesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/lotes",
                (string? search, Guid? galponId, EstadoLote? estado, int? pageNumber, int? pageSize,
                 string? sortBy, string? sortDir, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(
                        new SearchLotesQuery(search, galponId, estado, pageNumber ?? 1, pageSize ?? 20, sortBy, sortDir),
                        ct))
            .WithName("SearchLotes")
            .WithSummary("Search flocks (paged, sortable, filterable by shed/state)")
            .RequirePermission(AvicolaPermissions.Lotes.View);
    }
}
