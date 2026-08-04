using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.LinkInvoiceToProforma;

public static class LinkInvoiceToProformaEndpoint
{
    internal static RouteHandlerBuilder MapLinkInvoiceToProformaEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/proformas/link-invoice",
                async (LinkInvoiceToProformaCommand command, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(command, ct);
                    return Results.NoContent();
                })
            .WithName("LinkInvoiceToProforma")
            .WithSummary("Link (or unlink) an invoice to a proforma")
            .RequirePermission(CashflowPermissions.Proformas.Update);
    }
}
