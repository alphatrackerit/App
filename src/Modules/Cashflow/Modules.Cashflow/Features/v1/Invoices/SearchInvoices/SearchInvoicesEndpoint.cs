using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.SearchInvoices;

public static class SearchInvoicesEndpoint
{
    internal static RouteHandlerBuilder MapSearchInvoicesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/invoices",
                (string? search, int? pageNumber, int? pageSize, string? sortBy, string? sortDir,
                 InvoiceType? type, Guid? clientId, Guid? supplierId, Guid? companyId, Guid? projectId,
                 IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new SearchInvoicesQuery(
                        search, pageNumber ?? 1, pageSize ?? 20, sortBy, sortDir,
                        type, clientId, supplierId, companyId, projectId), ct))
            .WithName("SearchInvoices")
            .WithSummary("Search invoices")
            .RequirePermission(CashflowPermissions.Facturacion.View);
    }
}
