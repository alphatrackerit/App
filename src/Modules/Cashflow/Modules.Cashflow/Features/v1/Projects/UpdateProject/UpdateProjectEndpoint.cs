using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Projects;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Projects.UpdateProject;

public static class UpdateProjectEndpoint
{
    internal static RouteHandlerBuilder MapUpdateProjectEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/projects/{id:guid}",
                async (Guid id, UpdateProjectCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command with { ProjectId = id }, ct)))
            .WithName("UpdateProject")
            .WithSummary("Update a project")
            .RequirePermission(CashflowPermissions.Projects.Update);
    }
}
