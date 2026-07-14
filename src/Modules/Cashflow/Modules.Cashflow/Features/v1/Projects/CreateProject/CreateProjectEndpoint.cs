using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Projects;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Projects.CreateProject;

public static class CreateProjectEndpoint
{
    internal static RouteHandlerBuilder MapCreateProjectEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/projects",
                async (CreateProjectCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateProject")
            .WithSummary("Create a project")
            .RequirePermission(CashflowPermissions.Projects.Create)
            .WithIdempotency();
    }
}
