using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.AttachProformaDocument;

public static class AttachProformaDocumentEndpoint
{
    internal static RouteHandlerBuilder MapAttachProformaDocumentEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/proformas/{id:guid}/document",
                async (Guid id, IFormFile file, IMediator mediator, CancellationToken ct) =>
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms, ct);
                    string path = await mediator.Send(
                        new AttachProformaDocumentCommand(id, ms.ToArray(), file.FileName, file.ContentType), ct);
                    return Results.Ok(new { documentPath = path });
                })
            .WithName("AttachProformaDocument")
            .WithSummary("Attach or replace the source document of an existing proforma")
            .RequirePermission(CashflowPermissions.Proformas.Update)
            .DisableAntiforgery();
    }
}
