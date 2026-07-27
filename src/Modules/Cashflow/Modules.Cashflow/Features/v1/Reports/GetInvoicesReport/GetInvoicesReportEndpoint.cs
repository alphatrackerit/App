using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Reports;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Reports.GetInvoicesReport;

public static class GetInvoicesReportEndpoint
{
    internal static RouteHandlerBuilder MapGetInvoicesReportEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/reports/invoices",
            (InvoiceReportScope? scope, InvoiceType? type, Guid? clientId, Guid? supplierId, Guid? companyId,
             DateTime? from, DateTime? to, int? dueWithinDays, IMediator mediator, CancellationToken ct) =>
                mediator.Send(new GetInvoicesReportQuery(
                    scope ?? InvoiceReportScope.Todas, type, clientId, supplierId, companyId,
                    from, to, dueWithinDays), ct))
            .WithName("GetInvoicesReport")
            .WithSummary("Invoice report: overdue / upcoming / settled / pending, with amounts")
            .RequirePermission(CashflowPermissions.Reportes.View);
}
