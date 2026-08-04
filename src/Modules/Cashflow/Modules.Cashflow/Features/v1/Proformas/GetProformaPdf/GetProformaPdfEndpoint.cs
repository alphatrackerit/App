using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.GetProformaPdf;

public static class GetProformaPdfEndpoint
{
    internal static RouteHandlerBuilder MapGetProformaPdfEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/proformas/{id:guid}/pdf",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    var pdf = await mediator.Send(new GetProformaPdfQuery(id), ct);
                    return Results.File(pdf.Content, "application/pdf", pdf.FileName);
                })
            .WithName("GetProformaPdf")
            .WithSummary("Render the proforma as a PDF document (on demand, not persisted)")
            .RequirePermission(CashflowPermissions.Proformas.View);
    }
}
