using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.GetInvoiceLines;

public static class GetInvoiceLinesEndpoint
{
    internal static RouteHandlerBuilder MapGetInvoiceLinesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/invoices/{id:guid}/lines",
                (Guid id, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetInvoiceLinesQuery(id), ct))
            .WithName("GetInvoiceLines")
            .WithSummary("Get an invoice's cash lines and reconciliation totals")
            .RequirePermission(CashflowPermissions.Facturacion.View);
    }
}
