using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Documentos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Documentos.UpdateDocumento;

public static class UpdateDocumentoEndpoint
{
    internal static RouteHandlerBuilder MapUpdateDocumentoEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/documentos/{id:guid}",
                async (Guid id, UpdateDocumentoCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command with { DocumentoId = id }, ct)))
            .WithName("UpdateDocumento")
            .WithSummary("Update a document's type / notes")
            .RequirePermission(AvicolaPermissions.Documentos.Create);
    }
}
