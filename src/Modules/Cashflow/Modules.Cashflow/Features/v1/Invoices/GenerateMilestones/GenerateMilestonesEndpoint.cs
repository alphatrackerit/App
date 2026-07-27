using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.GenerateMilestones;

public static class GenerateMilestonesEndpoint
{
    internal static RouteHandlerBuilder MapGenerateMilestonesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/invoices/{id:guid}/generate-milestones",
                async (Guid id, GenerateMilestonesCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command with { InvoiceId = id }, ct)))
            .WithName("GenerateMilestones")
            .WithSummary("Generate forecast cash lines from the invoice's payment terms")
            .RequirePermission(CashflowPermissions.Facturacion.Create);
    }
}
