using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Alimentacion;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Alimentacion.UpdateAlimentacion;

public static class UpdateAlimentacionEndpoint
{
    internal static RouteHandlerBuilder MapUpdateAlimentacionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/alimentacion/{id:guid}",
                async (Guid id, UpdateAlimentacionCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command with { AlimentacionId = id }, ct)))
            .WithName("UpdateAlimentacion")
            .WithSummary("Update a feed record")
            .RequirePermission(AvicolaPermissions.Alimentacion.Update);
    }
}
