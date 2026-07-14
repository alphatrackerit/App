using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Documentos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Documentos.CreateDocumento;

public static class CreateDocumentoEndpoint
{
    internal static RouteHandlerBuilder MapCreateDocumentoEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/documentos",
                async (CreateDocumentoCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateDocumento")
            .WithSummary("Attach an uploaded document to an Avícola entity")
            .RequirePermission(AvicolaPermissions.Documentos.Create)
            .WithIdempotency();
    }
}
