using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.GetLinkSuggestions;

public static class GetLinkSuggestionsEndpoint
{
    internal static RouteHandlerBuilder MapGetLinkSuggestionsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/invoices/link-suggestions",
                (InvoiceType? type, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetLinkSuggestionsQuery(type), ct))
            .WithName("GetInvoiceLinkSuggestions")
            .WithSummary("Compute assisted invoice-line link suggestions (unambiguous only)")
            .RequirePermission(CashflowPermissions.Facturacion.View);
    }
}
