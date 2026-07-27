using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.ExtractInvoice;

public static class ExtractInvoiceEndpoint
{
    internal static RouteHandlerBuilder MapExtractInvoiceEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/invoices/extract",
                async (IFormFile file, IMediator mediator, CancellationToken ct) =>
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms, ct);
                    var result = await mediator.Send(
                        new ExtractInvoiceCommand(ms.ToArray(), file.FileName, file.ContentType), ct);
                    return Results.Ok(result);
                })
            .WithName("ExtractInvoiceFromDocument")
            .WithSummary("Upload an invoice document and get an AI-proposed data extraction")
            .RequirePermission(CashflowPermissions.Facturacion.Create)
            .DisableAntiforgery();
    }
}
