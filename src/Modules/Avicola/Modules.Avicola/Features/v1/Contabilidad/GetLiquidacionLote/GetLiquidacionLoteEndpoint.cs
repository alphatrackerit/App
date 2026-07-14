using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Contabilidad;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Contabilidad.GetLiquidacionLote;

public static class GetLiquidacionLoteEndpoint
{
    internal static RouteHandlerBuilder MapGetLiquidacionLoteEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/lotes/{id:guid}/liquidacion",
                (Guid id, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetLiquidacionLoteQuery(id), ct))
            .WithName("GetLiquidacionLote")
            .WithSummary("Full flock settlement (production close-out + financial result)")
            .RequirePermission(AvicolaPermissions.Contabilidad.View);
    }
}
