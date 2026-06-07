using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Projects.Contracts.Authorization;
using FSH.Modules.Projects.Contracts.v1.Projects;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Projects.Features.v1.Projects.SearchProjects;

public static class SearchProjectsEndpoint
{
    internal static RouteHandlerBuilder MapSearchProjectsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/projects",
                (string? search, int? pageNumber, int? pageSize, string? sortBy, string? sortDir,
                 IMediator mediator, CancellationToken ct) =>
                    mediator.Send(
                        new SearchProjectsQuery(
                            search,
                            pageNumber ?? 1,
                            pageSize ?? 20,
                            sortBy,
                            sortDir),
                        ct))
            .WithName("SearchProjects")
            .WithSummary("Search projects (paged, sortable)")
            .RequirePermission(ProjectsPermissions.Projects.View);
    }
}
