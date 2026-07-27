using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.AttachInvoiceDocument;

public static class AttachInvoiceDocumentEndpoint
{
    internal static RouteHandlerBuilder MapAttachInvoiceDocumentEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/invoices/{id:guid}/document",
                async (Guid id, IFormFile file, IMediator mediator, CancellationToken ct) =>
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms, ct);
                    string path = await mediator.Send(
                        new AttachInvoiceDocumentCommand(id, ms.ToArray(), file.FileName, file.ContentType), ct);
                    return Results.Ok(new { documentPath = path });
                })
            .WithName("AttachInvoiceDocument")
            .WithSummary("Attach or replace the digitalized document of an existing invoice")
            .RequirePermission(CashflowPermissions.Facturacion.Update)
            .DisableAntiforgery();
    }
}
