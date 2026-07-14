using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Lotes;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Lotes.CreateLote;

public static class CreateLoteEndpoint
{
    internal static RouteHandlerBuilder MapCreateLoteEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/lotes",
                async (CreateLoteCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateLote")
            .WithSummary("Create a flock (lote de engorde)")
            .RequirePermission(AvicolaPermissions.Lotes.Create)
            .WithIdempotency();
    }
}
