using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Sanidad;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Sanidad.UpdateSanidad;

public static class UpdateSanidadEndpoint
{
    internal static RouteHandlerBuilder MapUpdateSanidadEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/sanidad/{id:guid}",
                async (Guid id, UpdateSanidadCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command with { SanidadId = id }, ct)))
            .WithName("UpdateSanidad")
            .WithSummary("Update a health record")
            .RequirePermission(AvicolaPermissions.Sanidad.Update);
    }
}
