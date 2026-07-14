using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Prefixes;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Prefixes;

public static class SearchPrefixesEndpoint
{
    internal static RouteHandlerBuilder MapSearchPrefixesEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/prefixes",
            (string? search, PrefixType? type, bool? onlyActive, int? pageNumber, int? pageSize, string? sortBy, string? sortDir, IMediator mediator, CancellationToken ct) =>
                mediator.Send(new SearchPrefixesQuery(search, type, onlyActive, pageNumber ?? 1, pageSize ?? 20, sortBy, sortDir), ct))
            .WithName("SearchPrefixes")
            .WithSummary("Search prefixes (paged; filter by type / active)")
            .RequirePermission(CashflowPermissions.Prefixes.View);
}

public static class CreatePrefixEndpoint
{
    internal static RouteHandlerBuilder MapCreatePrefixEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/prefixes",
            async (CreatePrefixCommand command, IMediator mediator, CancellationToken ct) => Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreatePrefix")
            .WithSummary("Create a prefix (group or category)")
            .RequirePermission(CashflowPermissions.Prefixes.Create)
            .WithIdempotency();
}

public static class UpdatePrefixEndpoint
{
    internal static RouteHandlerBuilder MapUpdatePrefixEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPut("/prefixes/{id:guid}",
            async (Guid id, UpdatePrefixCommand command, IMediator mediator, CancellationToken ct) => Results.Ok(await mediator.Send(command with { Id = id }, ct)))
            .WithName("UpdatePrefix")
            .WithSummary("Update a prefix")
            .RequirePermission(CashflowPermissions.Prefixes.Update);
}

public static class DeletePrefixEndpoint
{
    internal static RouteHandlerBuilder MapDeletePrefixEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapDelete("/prefixes/{id:guid}",
            async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new DeletePrefixCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeletePrefix")
            .WithSummary("Delete a prefix")
            .RequirePermission(CashflowPermissions.Prefixes.Delete);
}
