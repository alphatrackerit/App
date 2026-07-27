using FSH.Modules.Cashflow.Contracts.Enums;

namespace FSH.Modules.Cashflow.Contracts.Dtos;

/// <summary>Settlement classification of one invoice inside the report.</summary>
public enum InvoiceReportStatus
{
    /// <summary>Fully covered by validated linked lines (cobrada / pagada).</summary>
    Settled = 0,
    /// <summary>Due date in the past with a pending amount (vencida).</summary>
    Overdue = 1,
    /// <summary>Due date today or later with a pending amount (por vencer).</summary>
    Upcoming = 2,
    /// <summary>Pending amount but no due date to judge it by (sin vencimiento).</summary>
    NoDueDate = 3,
}

/// <summary>One invoice row of the report, with counterparty/company names resolved and the
/// collected (validated linked) vs pending amounts computed.</summary>
public sealed record InvoiceReportRowDto(
    Guid Id,
    string Number,
    InvoiceType Type,
    string? CounterpartyName,
    string? CompanyName,
    DateTime? InvoiceDate,
    DateTime? DueDate,
    decimal Total,
    decimal Collected,
    decimal Pending,
    // Positive = days past due; negative = days until due. Null without due date.
    int? DaysOverdue,
    InvoiceReportStatus Status);

/// <summary>Count + amount of one report category. <c>Amount</c> is the pending amount for open
/// categories and the collected amount for <c>Settled</c>.</summary>
public sealed record InvoiceReportBucketDto(int Count, decimal Amount);

/// <summary>The invoice report: the rows of the requested scope plus category totals computed
/// over everything the filters matched (before the scope cut), so the summary is stable while
/// the user flips between scopes.</summary>
public sealed record InvoiceReportDto(
    DateTime GeneratedAt,
    int TotalCount,
    decimal TotalAmount,
    InvoiceReportBucketDto Settled,
    InvoiceReportBucketDto Pending,
    InvoiceReportBucketDto Overdue,
    InvoiceReportBucketDto Upcoming,
    IReadOnlyList<InvoiceReportRowDto> Rows);
