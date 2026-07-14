using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Alimentacion;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Alimentacion.SearchAlimentacion;

public static class SearchAlimentacionEndpoint
{
    internal static RouteHandlerBuilder MapSearchAlimentacionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/alimentacion",
                (Guid? loteId, TipoAlimento? tipoAlimento, int? pageNumber, int? pageSize, string? sortBy, string? sortDir,
                 IMediator mediator, CancellationToken ct) =>
                    mediator.Send(
                        new SearchAlimentacionQuery(loteId, tipoAlimento, pageNumber ?? 1, pageSize ?? 20, sortBy, sortDir),
                        ct))
            .WithName("SearchAlimentacion")
            .WithSummary("Search feed records (paged, sortable, filterable by flock/feed phase)")
            .RequirePermission(AvicolaPermissions.Alimentacion.View);
    }
}
