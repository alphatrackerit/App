using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Incomes;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Incomes.SearchIncomes;

public static class SearchIncomesEndpoint
{
    internal static RouteHandlerBuilder MapSearchIncomesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/incomes",
                (string? search, int? pageNumber, int? pageSize, string? sortBy, string? sortDir, Guid? projectId,
                 IMediator mediator, CancellationToken ct) =>
                    mediator.Send(
                        new SearchIncomesQuery(
                            search,
                            pageNumber ?? 1,
                            pageSize ?? 20,
                            sortBy,
                            sortDir,
                            projectId),
                        ct))
            .WithName("SearchIncomes")
            .WithSummary("Search incomes (paged, sortable, filter by project)")
            .RequirePermission(CashflowPermissions.Incomes.View);
    }
}
