using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Projects.Contracts.Authorization;
using FSH.Modules.Projects.Contracts.v1.Projects;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Projects.Features.v1.Projects.GetProjectById;

public static class GetProjectByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetProjectByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/projects/{id:guid}",
                (Guid id, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetProjectByIdQuery(id), ct))
            .WithName("GetProjectById")
            .WithSummary("Get a project by id")
            .RequirePermission(ProjectsPermissions.Projects.View);
    }
}
