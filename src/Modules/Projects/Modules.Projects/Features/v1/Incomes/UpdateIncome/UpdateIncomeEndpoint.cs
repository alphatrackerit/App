using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Projects.Contracts.Authorization;
using FSH.Modules.Projects.Contracts.v1.Incomes;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Projects.Features.v1.Incomes.UpdateIncome;

public static class UpdateIncomeEndpoint
{
    internal static RouteHandlerBuilder MapUpdateIncomeEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/incomes/{id:guid}",
                async (Guid id, UpdateIncomeCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command with { IncomeId = id }, ct)))
            .WithName("UpdateIncome")
            .WithSummary("Update an income")
            .RequirePermission(ProjectsPermissions.Incomes.Update);
    }
}
