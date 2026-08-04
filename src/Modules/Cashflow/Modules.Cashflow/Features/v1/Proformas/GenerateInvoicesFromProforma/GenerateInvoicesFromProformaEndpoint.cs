using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.GenerateInvoicesFromProforma;

public static class GenerateInvoicesFromProformaEndpoint
{
    internal static RouteHandlerBuilder MapGenerateInvoicesFromProformaEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/proformas/{id:guid}/generate-invoices",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new GenerateInvoicesFromProformaCommand(id), ct)))
            .WithName("GenerateInvoicesFromProforma")
            .WithSummary("Generate one draft invoice per payment-terms milestone of the proforma")
            .RequirePermission(CashflowPermissions.Proformas.GenerateInvoices)
            .WithIdempotency();
    }
}
