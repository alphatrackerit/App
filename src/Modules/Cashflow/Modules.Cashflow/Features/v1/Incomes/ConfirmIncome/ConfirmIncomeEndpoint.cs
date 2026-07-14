using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Incomes;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Incomes.ConfirmIncome;

public static class ConfirmIncomeEndpoint
{
    internal static RouteHandlerBuilder MapConfirmIncomeEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/incomes/{id:guid}/confirm",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new ConfirmIncomeCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("ConfirmIncome")
            .WithSummary("Mark an income as confirmed")
            .RequirePermission(CashflowPermissions.Facturacion.ConfirmIncome);
    }
}
