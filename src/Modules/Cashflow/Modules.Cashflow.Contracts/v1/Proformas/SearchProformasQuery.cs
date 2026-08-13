using FSH.Framework.Shared.Persistence;
using FSH.Modules.Cashflow.Contracts.Dtos;
using FSH.Modules.Cashflow.Contracts.Enums;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Proformas;

/// <summary>
/// Search for proformas with pagination and sorting.
/// </summary>
/// <param name="Search">Free-text term matched word-by-word against the proforma number, notes, the
/// names of its client / supplier / company / status, and (numeric words) the amounts.</param>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Page size (1-200).</param>
/// <param name="SortBy">Sort column. One of: number | total | date.</param>
/// <param name="SortDir">Sort direction. One of: asc | desc.</param>
/// <param name="Type">When set, only proformas of this type (issued/received).</param>
/// <param name="ClientId">When set, only proformas of this client.</param>
/// <param name="SupplierId">When set, only proformas of this supplier.</param>
/// <param name="CompanyId">When set, only proformas belonging to this company.</param>
/// <param name="ProjectId">When set, only proformas assigned to this project.</param>
/// <param name="Pending">When set, only proformas with pending invoicing work (no invoice, draft
/// numberless invoices, or partially invoiced).</param>
public sealed record SearchProformasQuery(
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null,
    InvoiceType? Type = null,
    Guid? ClientId = null,
    Guid? SupplierId = null,
    Guid? CompanyId = null,
    Guid? ProjectId = null,
    ProformaPendingFilter? Pending = null) : IQuery<PagedResponse<ProformaDto>>;
