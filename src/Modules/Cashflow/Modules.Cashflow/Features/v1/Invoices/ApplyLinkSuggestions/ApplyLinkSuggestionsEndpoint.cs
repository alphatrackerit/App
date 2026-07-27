using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.ApplyLinkSuggestions;

public static class ApplyLinkSuggestionsEndpoint
{
    internal static RouteHandlerBuilder MapApplyLinkSuggestionsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/invoices/link-suggestions/apply",
                async (ApplyLinkSuggestionsCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("ApplyInvoiceLinkSuggestions")
            .WithSummary("Apply a batch of accepted invoice-line link suggestions")
            .RequirePermission(CashflowPermissions.Facturacion.Update);
    }
}
