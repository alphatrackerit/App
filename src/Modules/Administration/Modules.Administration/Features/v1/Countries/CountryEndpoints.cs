using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Administration.Contracts.Authorization;
using FSH.Modules.Administration.Contracts.v1.Countries;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Administration.Features.v1.Countries;

public static class SearchCountriesEndpoint
{
    internal static RouteHandlerBuilder MapSearchCountriesEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/countries",
            (string? search, int? pageNumber, int? pageSize, string? sortBy, string? sortDir, IMediator mediator, CancellationToken ct) =>
                mediator.Send(new SearchCountriesQuery(search, pageNumber ?? 1, pageSize ?? 20, sortBy, sortDir), ct))
            .WithName("SearchCountries")
            .WithSummary("Search countries (paged)")
            .RequirePermission(AdministrationPermissions.Countries.View);
}

public static class CreateCountryEndpoint
{
    internal static RouteHandlerBuilder MapCreateCountryEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/countries",
            async (CreateCountryCommand command, IMediator mediator, CancellationToken ct) => Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateCountry")
            .WithSummary("Create a country")
            .RequirePermission(AdministrationPermissions.Countries.Create)
            .WithIdempotency();
}

public static class UpdateCountryEndpoint
{
    internal static RouteHandlerBuilder MapUpdateCountryEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPut("/countries/{id:guid}",
            async (Guid id, UpdateCountryCommand command, IMediator mediator, CancellationToken ct) => Results.Ok(await mediator.Send(command with { Id = id }, ct)))
            .WithName("UpdateCountry")
            .WithSummary("Update a country")
            .RequirePermission(AdministrationPermissions.Countries.Update);
}

public static class DeleteCountryEndpoint
{
    internal static RouteHandlerBuilder MapDeleteCountryEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapDelete("/countries/{id:guid}",
            async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new DeleteCountryCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteCountry")
            .WithSummary("Delete a country")
            .RequirePermission(AdministrationPermissions.Countries.Delete);
}
