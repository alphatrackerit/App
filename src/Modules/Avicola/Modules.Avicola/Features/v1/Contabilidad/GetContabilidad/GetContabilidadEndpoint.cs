using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Contabilidad;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Contabilidad.GetContabilidad;

public static class GetContabilidadEndpoint
{
    internal static RouteHandlerBuilder MapGetContabilidadEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/contabilidad",
                (Guid? loteId, DateTimeOffset? desde, DateTimeOffset? hasta, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetContabilidadQuery(loteId, desde, hasta), ct))
            .WithName("GetContabilidad")
            .WithSummary("Accounting overview: costs/revenue by category and by flock (computed + manual ledger)")
            .RequirePermission(AvicolaPermissions.Contabilidad.View);
    }
}
