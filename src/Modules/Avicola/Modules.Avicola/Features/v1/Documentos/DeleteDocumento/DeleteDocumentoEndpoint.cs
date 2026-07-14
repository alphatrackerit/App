using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Documentos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Documentos.DeleteDocumento;

public static class DeleteDocumentoEndpoint
{
    internal static RouteHandlerBuilder MapDeleteDocumentoEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/documentos/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new DeleteDocumentoCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("DeleteDocumento")
            .WithSummary("Delete a document")
            .RequirePermission(AvicolaPermissions.Documentos.Delete);
    }
}
