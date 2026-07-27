using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Incomes;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Incomes.ValidateIncome;

public static class ValidateIncomeEndpoint
{
    internal static RouteHandlerBuilder MapValidateIncomeEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/incomes/{id:guid}/validate",
                async (Guid id, bool? value, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new ValidateIncomeCommand(id, value ?? true), ct);
                    return Results.NoContent();
                })
            .WithName("ValidateIncome")
            .WithSummary("Set the validated flag of an income (value=false to clear)")
            .RequirePermission(CashflowPermissions.Facturacion.ValidateIncome);
    }
}
