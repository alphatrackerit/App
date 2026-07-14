using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Mortalidad;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Mortalidad.SearchMortalidad;

public static class SearchMortalidadEndpoint
{
    internal static RouteHandlerBuilder MapSearchMortalidadEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/mortalidad",
                (Guid? loteId, int? pageNumber, int? pageSize, string? sortBy, string? sortDir,
                 IMediator mediator, CancellationToken ct) =>
                    mediator.Send(
                        new SearchMortalidadQuery(loteId, pageNumber ?? 1, pageSize ?? 20, sortBy, sortDir),
                        ct))
            .WithName("SearchMortalidad")
            .WithSummary("Search mortality records (paged, sortable, filterable by flock)")
            .RequirePermission(AvicolaPermissions.Mortalidad.View);
    }
}
