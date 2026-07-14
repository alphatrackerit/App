using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Mortalidad;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Mortalidad.UpdateMortalidad;

public static class UpdateMortalidadEndpoint
{
    internal static RouteHandlerBuilder MapUpdateMortalidadEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/mortalidad/{id:guid}",
                async (Guid id, UpdateMortalidadCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command with { MortalidadId = id }, ct)))
            .WithName("UpdateMortalidad")
            .WithSummary("Update a mortality record")
            .RequirePermission(AvicolaPermissions.Mortalidad.Update);
    }
}
