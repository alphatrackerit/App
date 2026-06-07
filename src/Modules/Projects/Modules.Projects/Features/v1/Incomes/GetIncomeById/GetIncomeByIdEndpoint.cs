using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Projects.Contracts.Authorization;
using FSH.Modules.Projects.Contracts.v1.Incomes;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Projects.Features.v1.Incomes.GetIncomeById;

public static class GetIncomeByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetIncomeByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/incomes/{id:guid}",
                (Guid id, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetIncomeByIdQuery(id), ct))
            .WithName("GetIncomeById")
            .WithSummary("Get an income by id")
            .RequirePermission(ProjectsPermissions.Incomes.View);
    }
}
