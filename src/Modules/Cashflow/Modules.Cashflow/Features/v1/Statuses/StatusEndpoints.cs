using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Statuses;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Statuses;

public static class SearchStatusesEndpoint
{
    internal static RouteHandlerBuilder MapSearchStatusesEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/statuses",
            (string? search, int? pageNumber, int? pageSize, string? sortBy, string? sortDir, IMediator mediator, CancellationToken ct) =>
                mediator.Send(new SearchStatusesQuery(search, pageNumber ?? 1, pageSize ?? 20, sortBy, sortDir), ct))
            .WithName("SearchStatuses")
            .WithSummary("Search statuses (paged)")
            .RequirePermission(CashflowPermissions.Statuses.View);
}

public static class CreateStatusEndpoint
{
    internal static RouteHandlerBuilder MapCreateStatusEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/statuses",
            async (CreateStatusCommand command, IMediator mediator, CancellationToken ct) => Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateStatus")
            .WithSummary("Create a status")
            .RequirePermission(CashflowPermissions.Statuses.Create)
            .WithIdempotency();
}

public static class UpdateStatusEndpoint
{
    internal static RouteHandlerBuilder MapUpdateStatusEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPut("/statuses/{id:guid}",
            async (Guid id, UpdateStatusCommand command, IMediator mediator, CancellationToken ct) => Results.Ok(await mediator.Send(command with { Id = id }, ct)))
            .WithName("UpdateStatus")
            .WithSummary("Update a status")
            .RequirePermission(CashflowPermissions.Statuses.Update);
}

public static class DeleteStatusEndpoint
{
    internal static RouteHandlerBuilder MapDeleteStatusEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapDelete("/statuses/{id:guid}",
            async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new DeleteStatusCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteStatus")
            .WithSummary("Delete a status")
            .RequirePermission(CashflowPermissions.Statuses.Delete);
}
