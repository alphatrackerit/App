using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Clients;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Clients;

public static class SearchClientsEndpoint
{
    internal static RouteHandlerBuilder MapSearchClientsEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/clients",
            (string? search, int? pageNumber, int? pageSize, string? sortBy, string? sortDir, IMediator mediator, CancellationToken ct) =>
                mediator.Send(new SearchClientsQuery(search, pageNumber ?? 1, pageSize ?? 20, sortBy, sortDir), ct))
            .WithName("SearchClients")
            .WithSummary("Search clients (paged)")
            .RequirePermission(CashflowPermissions.Clients.View);
}

public static class CreateClientEndpoint
{
    internal static RouteHandlerBuilder MapCreateClientEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/clients",
            async (CreateClientCommand command, IMediator mediator, CancellationToken ct) => Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateClient")
            .WithSummary("Create a client")
            .RequirePermission(CashflowPermissions.Clients.Create)
            .WithIdempotency();
}

public static class UpdateClientEndpoint
{
    internal static RouteHandlerBuilder MapUpdateClientEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPut("/clients/{id:guid}",
            async (Guid id, UpdateClientCommand command, IMediator mediator, CancellationToken ct) => Results.Ok(await mediator.Send(command with { Id = id }, ct)))
            .WithName("UpdateClient")
            .WithSummary("Update a client")
            .RequirePermission(CashflowPermissions.Clients.Update);
}

public static class DeleteClientEndpoint
{
    internal static RouteHandlerBuilder MapDeleteClientEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapDelete("/clients/{id:guid}",
            async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new DeleteClientCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteClient")
            .WithSummary("Delete a client")
            .RequirePermission(CashflowPermissions.Clients.Delete);
}
