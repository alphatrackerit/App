using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Projects.Contracts.Authorization;
using FSH.Modules.Projects.Contracts.v1.Incomes;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Projects.Features.v1.Incomes.DeleteIncome;

public static class DeleteIncomeEndpoint
{
    internal static RouteHandlerBuilder MapDeleteIncomeEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/incomes/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new DeleteIncomeCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("DeleteIncome")
            .WithSummary("Delete an income")
            .RequirePermission(ProjectsPermissions.Incomes.Delete);
    }
}
