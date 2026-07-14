using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Preparaciones;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Preparaciones.SearchPreparaciones;

public static class SearchPreparacionesEndpoint
{
    internal static RouteHandlerBuilder MapSearchPreparacionesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/preparaciones",
                (Guid? galponId, EstadoPreparacion? estado, int? pageNumber, int? pageSize, string? sortBy, string? sortDir,
                 IMediator mediator, CancellationToken ct) =>
                    mediator.Send(
                        new SearchPreparacionesQuery(galponId, estado, pageNumber ?? 1, pageSize ?? 20, sortBy, sortDir),
                        ct))
            .WithName("SearchPreparaciones")
            .WithSummary("Search shed preparations (paged, filterable by shed/state)")
            .RequirePermission(AvicolaPermissions.Preparaciones.View);
    }
}
