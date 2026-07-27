using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.LinkLineToInvoice;

public static class LinkLineToInvoiceEndpoint
{
    internal static RouteHandlerBuilder MapLinkLineToInvoiceEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/invoices/link-line",
                async (LinkLineToInvoiceCommand command, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(command, ct);
                    return Results.NoContent();
                })
            .WithName("LinkLineToInvoice")
            .WithSummary("Link (or unlink) an income/payment line to an invoice")
            .RequirePermission(CashflowPermissions.Facturacion.Update);
    }
}
