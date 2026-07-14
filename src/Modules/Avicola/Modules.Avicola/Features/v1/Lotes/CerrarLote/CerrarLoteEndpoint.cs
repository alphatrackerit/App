using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Lotes;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Lotes.CerrarLote;

public static class CerrarLoteEndpoint
{
    internal static RouteHandlerBuilder MapCerrarLoteEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/lotes/{id:guid}/cerrar",
                async (Guid id, CerrarLoteCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command with { LoteId = id }, ct)))
            .WithName("CerrarLote")
            .WithSummary("Close a flock (record exit date and mark Finalizado)")
            .RequirePermission(AvicolaPermissions.Lotes.Cerrar);
    }
}
