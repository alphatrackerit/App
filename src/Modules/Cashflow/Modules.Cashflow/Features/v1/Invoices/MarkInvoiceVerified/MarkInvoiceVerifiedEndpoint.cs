using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.MarkInvoiceVerified;

public static class MarkInvoiceVerifiedEndpoint
{
    internal static RouteHandlerBuilder MapMarkInvoiceVerifiedEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/invoices/{id:guid}/verify",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new MarkInvoiceVerifiedCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("MarkInvoiceVerified")
            .WithSummary("Mark an invoice as verified against the master listing")
            .RequirePermission(CashflowPermissions.Facturacion.Update);
    }
}
