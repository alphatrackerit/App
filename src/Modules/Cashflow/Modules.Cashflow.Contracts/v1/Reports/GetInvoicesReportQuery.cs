using FSH.Modules.Cashflow.Contracts.Dtos;
using FSH.Modules.Cashflow.Contracts.Enums;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Reports;

/// <summary>
/// Invoice report (facturación): every invoice matching the filters, classified against its
/// linked validated cash lines — settled (cobrada/pagada), overdue (vencida), upcoming
/// (por vencer) or without due date. All filters optional; <c>From</c>/<c>To</c> bound the
/// invoice date (day-granular, <c>To</c> inclusive). <c>DueWithinDays</c> narrows the
/// <see cref="InvoiceReportScope.PorVencer"/> scope to due dates inside that horizon.
/// </summary>
public sealed record GetInvoicesReportQuery(
    InvoiceReportScope Scope = InvoiceReportScope.Todas,
    InvoiceType? Type = null,
    Guid? ClientId = null,
    Guid? SupplierId = null,
    Guid? CompanyId = null,
    DateTime? From = null,
    DateTime? To = null,
    int? DueWithinDays = null) : IQuery<InvoiceReportDto>;

/// <summary>Which slice of invoices the report returns.</summary>
public enum InvoiceReportScope
{
    Todas = 0,
    /// <summary>Due date in the past and not fully settled.</summary>
    Vencidas = 1,
    /// <summary>Due date today or later (optionally within <c>DueWithinDays</c>) and not settled.</summary>
    PorVencer = 2,
    /// <summary>Fully covered by validated linked lines (cobrada / pagada).</summary>
    Cobradas = 3,
    /// <summary>Anything with a pending amount (por cobrar / por pagar), overdue or not.</summary>
    PorCobrar = 4,
}
