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
                async (Guid id, bool? value, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new ConfirmIncomeCommand(id, value ?? true), ct);
                    return Results.NoContent();
                })
            .WithName("ConfirmIncome")
            .WithSummary("Set the confirmed flag of an income (value=false to clear)")
            .RequirePermission(CashflowPermissions.Facturacion.ConfirmIncome);
    }
}
