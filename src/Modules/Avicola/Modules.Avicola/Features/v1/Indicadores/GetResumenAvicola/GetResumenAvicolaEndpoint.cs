using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Indicadores;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Indicadores.GetResumenAvicola;

public static class GetResumenAvicolaEndpoint
{
    internal static RouteHandlerBuilder MapGetResumenAvicolaEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/resumen",
                (IMediator mediator, CancellationToken ct) => mediator.Send(new GetResumenAvicolaQuery(), ct))
            .WithName("GetResumenAvicola")
            .WithSummary("Cross-flock dashboard summary: per-flock headline KPIs plus tenant-wide rollups")
            .RequirePermission(AvicolaPermissions.Lotes.View);
    }
}
