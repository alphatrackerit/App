using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Administration.Contracts.Authorization;
using FSH.Modules.Administration.Contracts.v1.Suppliers;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Administration.Features.v1.Suppliers;

public static class SearchSuppliersEndpoint
{
    internal static RouteHandlerBuilder MapSearchSuppliersEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/suppliers",
            (string? search, int? pageNumber, int? pageSize, string? sortBy, string? sortDir, IMediator mediator, CancellationToken ct) =>
                mediator.Send(new SearchSuppliersQuery(search, pageNumber ?? 1, pageSize ?? 20, sortBy, sortDir), ct))
            .WithName("SearchSuppliers")
            .WithSummary("Search suppliers (paged)")
            .RequirePermission(AdministrationPermissions.Suppliers.View);
}

public static class CreateSupplierEndpoint
{
    internal static RouteHandlerBuilder MapCreateSupplierEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/suppliers",
            async (CreateSupplierCommand command, IMediator mediator, CancellationToken ct) => Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateSupplier")
            .WithSummary("Create a supplier")
            .RequirePermission(AdministrationPermissions.Suppliers.Create)
            .WithIdempotency();
}

public static class UpdateSupplierEndpoint
{
    internal static RouteHandlerBuilder MapUpdateSupplierEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPut("/suppliers/{id:guid}",
            async (Guid id, UpdateSupplierCommand command, IMediator mediator, CancellationToken ct) => Results.Ok(await mediator.Send(command with { Id = id }, ct)))
            .WithName("UpdateSupplier")
            .WithSummary("Update a supplier")
            .RequirePermission(AdministrationPermissions.Suppliers.Update);
}

public static class DeleteSupplierEndpoint
{
    internal static RouteHandlerBuilder MapDeleteSupplierEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapDelete("/suppliers/{id:guid}",
            async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new DeleteSupplierCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteSupplier")
            .WithSummary("Delete a supplier")
            .RequirePermission(AdministrationPermissions.Suppliers.Delete);
}
