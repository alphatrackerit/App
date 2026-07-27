using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.CreateInvoice;

public static class CreateInvoiceEndpoint
{
    internal static RouteHandlerBuilder MapCreateInvoiceEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/invoices",
                async (CreateInvoiceCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateInvoice")
            .WithSummary("Create an invoice")
            .RequirePermission(CashflowPermissions.Facturacion.Create)
            .WithIdempotency();
    }
}
