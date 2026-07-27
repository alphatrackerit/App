using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Companies;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Companies;

public static class SearchCompaniesEndpoint
{
    internal static RouteHandlerBuilder MapSearchCompaniesEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/companies",
            (string? search, int? pageNumber, int? pageSize, string? sortBy, string? sortDir, bool? showInProjects, IMediator mediator, CancellationToken ct) =>
                mediator.Send(new SearchCompaniesQuery(search, pageNumber ?? 1, pageSize ?? 20, sortBy, sortDir, showInProjects), ct))
            .WithName("SearchCompanies")
            .WithSummary("Search companies (paged)")
            .RequirePermission(CashflowPermissions.Companies.View);
}

public static class CreateCompanyEndpoint
{
    internal static RouteHandlerBuilder MapCreateCompanyEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/companies",
            async (CreateCompanyCommand command, IMediator mediator, CancellationToken ct) => Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateCompany")
            .WithSummary("Create a company")
            .RequirePermission(CashflowPermissions.Companies.Create)
            .WithIdempotency();
}

public static class UpdateCompanyEndpoint
{
    internal static RouteHandlerBuilder MapUpdateCompanyEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPut("/companies/{id:guid}",
            async (Guid id, UpdateCompanyCommand command, IMediator mediator, CancellationToken ct) => Results.Ok(await mediator.Send(command with { Id = id }, ct)))
            .WithName("UpdateCompany")
            .WithSummary("Update a company")
            .RequirePermission(CashflowPermissions.Companies.Update);
}

public static class DeleteCompanyEndpoint
{
    internal static RouteHandlerBuilder MapDeleteCompanyEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapDelete("/companies/{id:guid}",
            async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new DeleteCompanyCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteCompany")
            .WithSummary("Delete a company")
            .RequirePermission(CashflowPermissions.Companies.Delete);
}
