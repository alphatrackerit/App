using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.BankMovements;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.BankMovements;

public static class SearchBankMovementsEndpoint
{
    internal static RouteHandlerBuilder MapSearchBankMovementsEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/bank-movements",
            (string? search, int? pageNumber, int? pageSize, string? sortBy, string? sortDir, IMediator mediator, CancellationToken ct) =>
                mediator.Send(new SearchBankMovementsQuery(search, pageNumber ?? 1, pageSize ?? 20, sortBy, sortDir), ct))
            .WithName("SearchBankMovements")
            .WithSummary("Search bank movements (paged)")
            .RequirePermission(CashflowPermissions.BankMovements.View);
}

public static class CreateBankMovementEndpoint
{
    internal static RouteHandlerBuilder MapCreateBankMovementEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/bank-movements",
            async (CreateBankMovementCommand command, IMediator mediator, CancellationToken ct) => Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateBankMovement")
            .WithSummary("Create a bank movement")
            .RequirePermission(CashflowPermissions.BankMovements.Create)
            .WithIdempotency();
}

public static class UpdateBankMovementEndpoint
{
    internal static RouteHandlerBuilder MapUpdateBankMovementEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPut("/bank-movements/{id:guid}",
            async (Guid id, UpdateBankMovementCommand command, IMediator mediator, CancellationToken ct) => Results.Ok(await mediator.Send(command with { Id = id }, ct)))
            .WithName("UpdateBankMovement")
            .WithSummary("Update a bank movement")
            .RequirePermission(CashflowPermissions.BankMovements.Update);
}

public static class DeleteBankMovementEndpoint
{
    internal static RouteHandlerBuilder MapDeleteBankMovementEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapDelete("/bank-movements/{id:guid}",
            async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new DeleteBankMovementCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteBankMovement")
            .WithSummary("Delete a bank movement")
            .RequirePermission(CashflowPermissions.BankMovements.Delete);
}
