using FSH.Framework.Shared.Persistence;
using FSH.Modules.Cashflow.Contracts.Dtos;
using FSH.Modules.Cashflow.Contracts.Enums;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Invoices;

/// <summary>
/// Search for invoices with pagination and sorting.
/// </summary>
/// <param name="Search">Free-text term matched word-by-word against the invoice number, Dynamics number,
/// bank, notes, the names of its client / supplier / company / status, and (numeric words) the amounts.</param>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Page size (1-200).</param>
/// <param name="SortBy">Sort column. One of: number | total | dueDate | invoiceDate.</param>
/// <param name="SortDir">Sort direction. One of: asc | desc.</param>
/// <param name="Type">When set, only invoices of this type (issued/received).</param>
/// <param name="ClientId">When set, only invoices of this client.</param>
/// <param name="SupplierId">When set, only invoices of this supplier.</param>
/// <param name="CompanyId">When set, only invoices belonging to this company.</param>
/// <param name="ProjectId">When set, only invoices assigned to this project.</param>
public sealed record SearchInvoicesQuery(
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null,
    InvoiceType? Type = null,
    Guid? ClientId = null,
    Guid? SupplierId = null,
    Guid? CompanyId = null,
    Guid? ProjectId = null) : IQuery<PagedResponse<InvoiceDto>>;
