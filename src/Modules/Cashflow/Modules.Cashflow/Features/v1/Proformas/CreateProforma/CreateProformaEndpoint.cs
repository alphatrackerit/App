using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.CreateProforma;

public static class CreateProformaEndpoint
{
    internal static RouteHandlerBuilder MapCreateProformaEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/proformas",
                async (CreateProformaCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateProforma")
            .WithSummary("Create a proforma")
            .RequirePermission(CashflowPermissions.Proformas.Create)
            .WithIdempotency();
    }
}
