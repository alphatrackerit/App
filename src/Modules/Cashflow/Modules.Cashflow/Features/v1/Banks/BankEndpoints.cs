using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Banks;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Banks;

public static class SearchBanksEndpoint
{
    internal static RouteHandlerBuilder MapSearchBanksEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/banks",
            (string? search, int? pageNumber, int? pageSize, string? sortBy, string? sortDir, IMediator mediator, CancellationToken ct) =>
                mediator.Send(new SearchBanksQuery(search, pageNumber ?? 1, pageSize ?? 20, sortBy, sortDir), ct))
            .WithName("SearchBanks")
            .WithSummary("Search banks (paged)")
            .RequirePermission(CashflowPermissions.Banks.View);
}

public static class CreateBankEndpoint
{
    internal static RouteHandlerBuilder MapCreateBankEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/banks",
            async (CreateBankCommand command, IMediator mediator, CancellationToken ct) => Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateBank")
            .WithSummary("Create a bank")
            .RequirePermission(CashflowPermissions.Banks.Create)
            .WithIdempotency();
}

public static class UpdateBankEndpoint
{
    internal static RouteHandlerBuilder MapUpdateBankEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPut("/banks/{id:guid}",
            async (Guid id, UpdateBankCommand command, IMediator mediator, CancellationToken ct) => Results.Ok(await mediator.Send(command with { Id = id }, ct)))
            .WithName("UpdateBank")
            .WithSummary("Update a bank")
            .RequirePermission(CashflowPermissions.Banks.Update);
}

public static class DeleteBankEndpoint
{
    internal static RouteHandlerBuilder MapDeleteBankEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapDelete("/banks/{id:guid}",
            async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new DeleteBankCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteBank")
            .WithSummary("Delete a bank")
            .RequirePermission(CashflowPermissions.Banks.Delete);
}
