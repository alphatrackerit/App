using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.SearchProformas;

public static class SearchProformasEndpoint
{
    internal static RouteHandlerBuilder MapSearchProformasEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/proformas",
                (string? search, int? pageNumber, int? pageSize, string? sortBy, string? sortDir,
                 InvoiceType? type, Guid? clientId, Guid? supplierId, Guid? companyId, Guid? projectId,
                 IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new SearchProformasQuery(
                        search, pageNumber ?? 1, pageSize ?? 20, sortBy, sortDir,
                        type, clientId, supplierId, companyId, projectId), ct))
            .WithName("SearchProformas")
            .WithSummary("Search proformas")
            .RequirePermission(CashflowPermissions.Proformas.View);
    }
}
