using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Lotes;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Lotes.UpdateLote;

public static class UpdateLoteEndpoint
{
    internal static RouteHandlerBuilder MapUpdateLoteEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/lotes/{id:guid}",
                async (Guid id, UpdateLoteCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command with { LoteId = id }, ct)))
            .WithName("UpdateLote")
            .WithSummary("Update a flock")
            .RequirePermission(AvicolaPermissions.Lotes.Update);
    }
}
