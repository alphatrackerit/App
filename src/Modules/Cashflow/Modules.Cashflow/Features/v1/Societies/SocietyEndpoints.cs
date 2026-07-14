using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Societies;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Societies;

public static class SearchSocietiesEndpoint
{
    internal static RouteHandlerBuilder MapSearchSocietiesEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/societies",
            (string? search, Guid? clientId, int? pageNumber, int? pageSize, string? sortBy, string? sortDir, IMediator mediator, CancellationToken ct) =>
                mediator.Send(new SearchSocietiesQuery(search, clientId, pageNumber ?? 1, pageSize ?? 20, sortBy, sortDir), ct))
            .WithName("SearchSocieties")
            .WithSummary("Search societies (paged)")
            .RequirePermission(CashflowPermissions.Societies.View);
}

public static class CreateSocietyEndpoint
{
    internal static RouteHandlerBuilder MapCreateSocietyEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/societies",
            async (CreateSocietyCommand command, IMediator mediator, CancellationToken ct) => Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateSociety")
            .WithSummary("Create a society")
            .RequirePermission(CashflowPermissions.Societies.Create)
            .WithIdempotency();
}

public static class UpdateSocietyEndpoint
{
    internal static RouteHandlerBuilder MapUpdateSocietyEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPut("/societies/{id:guid}",
            async (Guid id, UpdateSocietyCommand command, IMediator mediator, CancellationToken ct) => Results.Ok(await mediator.Send(command with { Id = id }, ct)))
            .WithName("UpdateSociety")
            .WithSummary("Update a society")
            .RequirePermission(CashflowPermissions.Societies.Update);
}

public static class DeleteSocietyEndpoint
{
    internal static RouteHandlerBuilder MapDeleteSocietyEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapDelete("/societies/{id:guid}",
            async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new DeleteSocietyCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteSociety")
            .WithSummary("Delete a society")
            .RequirePermission(CashflowPermissions.Societies.Delete);
}
