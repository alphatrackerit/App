using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.DeleteProforma;

public static class DeleteProformaEndpoint
{
    internal static RouteHandlerBuilder MapDeleteProformaEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/proformas/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new DeleteProformaCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("DeleteProforma")
            .WithSummary("Delete a proforma (linked invoices are unlinked, not deleted)")
            .RequirePermission(CashflowPermissions.Proformas.Delete);
    }
}
