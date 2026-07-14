using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Indicadores;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Indicadores.GetIndicadoresLote;

public static class GetIndicadoresLoteEndpoint
{
    internal static RouteHandlerBuilder MapGetIndicadoresLoteEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/lotes/{id:guid}/indicadores",
                (Guid id, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetIndicadoresLoteQuery(id), ct))
            .WithName("GetIndicadoresLote")
            .WithSummary("Production KPIs for a flock (mortality %, FCR, ADG, viability, IEP, costs)")
            .RequirePermission(AvicolaPermissions.Lotes.View);
    }
}
