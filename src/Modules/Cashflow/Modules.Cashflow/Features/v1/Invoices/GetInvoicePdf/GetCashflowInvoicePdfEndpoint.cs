using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.GetInvoicePdf;

public static class GetCashflowInvoicePdfEndpoint
{
    internal static RouteHandlerBuilder MapGetCashflowInvoicePdfEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/invoices/{id:guid}/pdf",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    var pdf = await mediator.Send(new GetCashflowInvoicePdfQuery(id), ct);
                    return Results.File(pdf.Content, "application/pdf", pdf.FileName);
                })
            // "GetCashflowInvoicePdf": endpoint names are GLOBAL and Billing owns "GetInvoicePdf".
            .WithName("GetCashflowInvoicePdf")
            .WithSummary("Render the invoice as a presentable PDF (with AEAT QR when VeriFactu-registered)")
            .RequirePermission(CashflowPermissions.Facturacion.View);
    }
}
