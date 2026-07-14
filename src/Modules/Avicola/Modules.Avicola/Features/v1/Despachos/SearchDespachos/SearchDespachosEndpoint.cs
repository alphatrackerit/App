using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Despachos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Despachos.SearchDespachos;

public static class SearchDespachosEndpoint
{
    internal static RouteHandlerBuilder MapSearchDespachosEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/despachos",
                (Guid? loteId, int? pageNumber, int? pageSize, string? sortBy, string? sortDir,
                 IMediator mediator, CancellationToken ct) =>
                    mediator.Send(
                        new SearchDespachosQuery(loteId, pageNumber ?? 1, pageSize ?? 20, sortBy, sortDir),
                        ct))
            .WithName("SearchDespachos")
            .WithSummary("Search dispatch records (paged, sortable, filterable by flock)")
            .RequirePermission(AvicolaPermissions.Despachos.View);
    }
}
