using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.DeleteInvoice;

public static class DeleteInvoiceEndpoint
{
    internal static RouteHandlerBuilder MapDeleteInvoiceEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/invoices/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new DeleteInvoiceCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("DeleteInvoice")
            .WithSummary("Delete an invoice")
            .RequirePermission(CashflowPermissions.Facturacion.Delete);
    }
}
