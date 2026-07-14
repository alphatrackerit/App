using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Sanidad;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Sanidad.SearchSanidad;

public static class SearchSanidadEndpoint
{
    internal static RouteHandlerBuilder MapSearchSanidadEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/sanidad",
                (Guid? loteId, TipoRegistroSanitario? tipo, int? pageNumber, int? pageSize, string? sortBy, string? sortDir,
                 IMediator mediator, CancellationToken ct) =>
                    mediator.Send(
                        new SearchSanidadQuery(loteId, tipo, pageNumber ?? 1, pageSize ?? 20, sortBy, sortDir),
                        ct))
            .WithName("SearchSanidad")
            .WithSummary("Search health records (paged, sortable, filterable by flock/type)")
            .RequirePermission(AvicolaPermissions.Sanidad.View);
    }
}
