using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Preparaciones;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Preparaciones.CompletarPreparacion;

public static class CompletarPreparacionEndpoint
{
    internal static RouteHandlerBuilder MapCompletarPreparacionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/preparaciones/{id:guid}/completar",
                async (Guid id, CompletarPreparacionCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command with { PreparacionId = id }, ct)))
            .WithName("CompletarPreparacion")
            .WithSummary("Mark a shed preparation complete (ready for the next flock)")
            .RequirePermission(AvicolaPermissions.Preparaciones.Completar);
    }
}
