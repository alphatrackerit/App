using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.UpdateProforma;

public static class UpdateProformaEndpoint
{
    internal static RouteHandlerBuilder MapUpdateProformaEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/proformas/{id:guid}",
                async (Guid id, UpdateProformaCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command with { ProformaId = id }, ct)))
            .WithName("UpdateProforma")
            .WithSummary("Update a proforma")
            .RequirePermission(CashflowPermissions.Proformas.Update);
    }
}
