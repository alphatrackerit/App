using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Verifactu;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.IssueInvoiceVerifactu;

public static class IssueInvoiceVerifactuEndpoint
{
    internal static RouteHandlerBuilder MapIssueInvoiceVerifactuEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/invoices/{id:guid}/verifactu/issue",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new IssueInvoiceVerifactuCommand(id), ct)))
            .WithName("IssueInvoiceVerifactu")
            .WithSummary("Issue the invoice under VERI*FACTU (chained record + QR; irreversible)")
            .RequirePermission(CashflowPermissions.Facturacion.IssueVerifactu)
            .WithIdempotency();
    }
}
