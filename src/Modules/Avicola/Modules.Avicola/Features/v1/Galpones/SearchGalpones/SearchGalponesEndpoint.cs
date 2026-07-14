using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Galpones;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Galpones.SearchGalpones;

public static class SearchGalponesEndpoint
{
    internal static RouteHandlerBuilder MapSearchGalponesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/galpones",
                (string? search, bool? activo, int? pageNumber, int? pageSize, string? sortBy, string? sortDir,
                 IMediator mediator, CancellationToken ct) =>
                    mediator.Send(
                        new SearchGalponesQuery(search, activo, pageNumber ?? 1, pageSize ?? 20, sortBy, sortDir),
                        ct))
            .WithName("SearchGalpones")
            .WithSummary("Search sheds (paged, sortable)")
            .RequirePermission(AvicolaPermissions.Galpones.View);
    }
}
