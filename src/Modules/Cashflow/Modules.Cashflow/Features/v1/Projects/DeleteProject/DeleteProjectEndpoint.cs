using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Projects;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Projects.DeleteProject;

public static class DeleteProjectEndpoint
{
    internal static RouteHandlerBuilder MapDeleteProjectEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/projects/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new DeleteProjectCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("DeleteProject")
            .WithSummary("Delete a project")
            .RequirePermission(CashflowPermissions.Projects.Delete);
    }
}
