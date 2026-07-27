using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.UpdateInvoice;

public static class UpdateInvoiceEndpoint
{
    internal static RouteHandlerBuilder MapUpdateInvoiceEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/invoices/{id:guid}",
                async (Guid id, UpdateInvoiceCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command with { InvoiceId = id }, ct)))
            .WithName("UpdateInvoice")
            .WithSummary("Update an invoice")
            .RequirePermission(CashflowPermissions.Facturacion.Update);
    }
}
