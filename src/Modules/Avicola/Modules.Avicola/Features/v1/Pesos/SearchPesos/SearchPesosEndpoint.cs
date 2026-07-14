using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Pesos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Pesos.SearchPesos;

public static class SearchPesosEndpoint
{
    internal static RouteHandlerBuilder MapSearchPesosEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/pesos",
                (Guid? loteId, int? pageNumber, int? pageSize, string? sortBy, string? sortDir,
                 IMediator mediator, CancellationToken ct) =>
                    mediator.Send(
                        new SearchPesosQuery(loteId, pageNumber ?? 1, pageSize ?? 20, sortBy, sortDir),
                        ct))
            .WithName("SearchPesos")
            .WithSummary("Search weight records (paged, sortable, filterable by flock)")
            .RequirePermission(AvicolaPermissions.Pesos.View);
    }
}
