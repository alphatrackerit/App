using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.GetInvoiceById;

public static class GetInvoiceByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetInvoiceByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/invoices/{id:guid}",
                (Guid id, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetInvoiceByIdQuery(id), ct))
            // "GetCashflowInvoiceById": endpoint names are GLOBAL across modules and Billing already
            // owns "GetInvoiceById" — a duplicate breaks route-matcher construction (500 on every request).
            .WithName("GetCashflowInvoiceById")
            .WithSummary("Get an invoice by id")
            .RequirePermission(CashflowPermissions.Facturacion.View);
    }
}
