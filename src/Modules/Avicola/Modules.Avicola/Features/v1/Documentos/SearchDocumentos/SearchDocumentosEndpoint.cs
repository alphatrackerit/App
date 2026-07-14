using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Avicola.Contracts;
using FSH.Modules.Avicola.Contracts.Authorization;
using FSH.Modules.Avicola.Contracts.v1.Documentos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Avicola.Features.v1.Documentos.SearchDocumentos;

public static class SearchDocumentosEndpoint
{
    internal static RouteHandlerBuilder MapSearchDocumentosEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/documentos",
                (DocumentoOrigen? origen, Guid? origenId, TipoDocumento? tipo, int? pageNumber, int? pageSize,
                 string? sortBy, string? sortDir, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(
                        new SearchDocumentosQuery(origen, origenId, tipo, pageNumber ?? 1, pageSize ?? 20, sortBy, sortDir),
                        ct))
            .WithName("SearchDocumentos")
            .WithSummary("Search documents (paged, filterable by owner entity/type)")
            .RequirePermission(AvicolaPermissions.Documentos.View);
    }
}
