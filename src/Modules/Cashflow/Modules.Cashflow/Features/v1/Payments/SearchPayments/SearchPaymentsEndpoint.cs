using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Payments;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Payments.SearchPayments;

public static class SearchPaymentsEndpoint
{
    internal static RouteHandlerBuilder MapSearchPaymentsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/payments",
                (string? search, int? pageNumber, int? pageSize, string? sortBy, string? sortDir, Guid? projectId,
                 bool? unlinked, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(
                        new SearchPaymentsQuery(
                            search,
                            pageNumber ?? 1,
                            pageSize ?? 20,
                            sortBy,
                            sortDir,
                            projectId,
                            unlinked),
                        ct))
            .WithName("SearchPayments")
            .WithSummary("Search payments (paged, sortable, filter by project)")
            .RequirePermission(CashflowPermissions.Payments.View);
    }
}
