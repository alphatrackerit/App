using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Projects.Contracts.Authorization;
using FSH.Modules.Projects.Contracts.v1.Incomes;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Projects.Features.v1.Incomes.CreateIncome;

public static class CreateIncomeEndpoint
{
    internal static RouteHandlerBuilder MapCreateIncomeEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/incomes",
                async (CreateIncomeCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateIncome")
            .WithSummary("Create an income")
            .RequirePermission(ProjectsPermissions.Incomes.Create)
            .WithIdempotency();
    }
}
