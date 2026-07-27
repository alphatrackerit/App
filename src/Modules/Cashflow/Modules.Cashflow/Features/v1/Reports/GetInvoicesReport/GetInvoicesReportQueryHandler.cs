using FSH.Modules.Cashflow.Contracts.Dtos;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Reports;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Reports.GetInvoicesReport;

public sealed class GetInvoicesReportQueryHandler(CashflowDbContext dbContext)
    : IQueryHandler<GetInvoicesReportQuery, InvoiceReportDto>
{
    // Amounts within half a cent of the total count as fully settled (rounding noise in legacy data).
    private const decimal SettleTolerance = 0.005m;

    public async ValueTask<InvoiceReportDto> Handle(GetInvoicesReportQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var q = dbContext.Invoices.AsNoTracking().AsQueryable();

        if (query.Type.HasValue)
        {
            q = q.Where(i => i.Type == query.Type.Value);
        }

        if (query.ClientId.HasValue)
        {
            q = q.Where(i => i.ClientId == query.ClientId.Value);
        }

        if (query.SupplierId.HasValue)
        {
            q = q.Where(i => i.SupplierId == query.SupplierId.Value);
        }

        if (query.CompanyId.HasValue)
        {
            q = q.Where(i => i.CompanyId == query.CompanyId.Value);
        }

        if (query.From.HasValue)
        {
            DateTime from = query.From.Value.Date;
            q = q.Where(i => i.InvoiceDate != null && i.InvoiceDate >= from);
        }

        if (query.To.HasValue)
        {
            DateTime toExclusive = query.To.Value.Date.AddDays(1);
            q = q.Where(i => i.InvoiceDate != null && i.InvoiceDate < toExclusive);
        }

        var invoices = await q
            .Select(i => new { i.Id, i.Number, i.Type, i.InvoiceDate, i.DueDate, i.ClientId, i.SupplierId, i.CompanyId, i.Total })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        // Validated linked amounts per invoice — one grouped query per line table.
        var incomeSums = await dbContext.Incomes.AsNoTracking()
            .Where(x => x.InvoiceId != null && x.Validated)
            .GroupBy(x => x.InvoiceId!.Value)
            .Select(g => new { g.Key, Sum = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.Key, x => x.Sum, cancellationToken).ConfigureAwait(false);
        var paymentSums = await dbContext.Payments.AsNoTracking()
            .Where(x => x.InvoiceId != null && x.Validated)
            .GroupBy(x => x.InvoiceId!.Value)
            .Select(g => new { g.Key, Sum = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.Key, x => x.Sum, cancellationToken).ConfigureAwait(false);

        var clientNames = await dbContext.Clients.AsNoTracking()
            .Select(c => new { c.Id, c.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken).ConfigureAwait(false);
        var supplierNames = await dbContext.Suppliers.AsNoTracking()
            .Select(s => new { s.Id, s.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken).ConfigureAwait(false);
        var companyNames = await dbContext.Companies.AsNoTracking()
            .Select(c => new { c.Id, c.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken).ConfigureAwait(false);

        DateTime today = DateTime.UtcNow.Date;
        var rows = new List<InvoiceReportRowDto>(invoices.Count);
        foreach (var i in invoices)
        {
            var sums = i.Type == InvoiceType.Emitida ? incomeSums : paymentSums;
            decimal collected = sums.TryGetValue(i.Id, out decimal s) ? s : 0m;
            decimal pending = i.Total - collected;

            InvoiceReportStatus status;
            if (pending <= SettleTolerance)
            {
                status = InvoiceReportStatus.Settled;
            }
            else if (i.DueDate is null)
            {
                status = InvoiceReportStatus.NoDueDate;
            }
            else
            {
                status = i.DueDate.Value.Date < today ? InvoiceReportStatus.Overdue : InvoiceReportStatus.Upcoming;
            }

            int? daysOverdue = i.DueDate is null ? null : (int)(today - i.DueDate.Value.Date).TotalDays;

            string? counterparty = null;
            if (i.Type == InvoiceType.Emitida)
            {
                if (i.ClientId is Guid cid && clientNames.TryGetValue(cid, out string? cn)) counterparty = cn;
            }
            else if (i.SupplierId is Guid sid && supplierNames.TryGetValue(sid, out string? sn))
            {
                counterparty = sn;
            }
            string? company = i.CompanyId is Guid coid && companyNames.TryGetValue(coid, out string? con) ? con : null;

            rows.Add(new InvoiceReportRowDto(
                i.Id, i.Number, i.Type, counterparty, company, i.InvoiceDate, i.DueDate,
                i.Total, collected, pending, daysOverdue, status));
        }

        // Summary buckets over everything the filters matched, before the scope cut.
        InvoiceReportBucketDto Bucket(Func<InvoiceReportRowDto, bool> match, bool settled = false)
        {
            var hit = rows.Where(match).ToList();
            return new InvoiceReportBucketDto(hit.Count, hit.Sum(r => settled ? r.Collected : r.Pending));
        }

        var summarySettled = Bucket(r => r.Status == InvoiceReportStatus.Settled, settled: true);
        var summaryPending = Bucket(r => r.Status != InvoiceReportStatus.Settled);
        var summaryOverdue = Bucket(r => r.Status == InvoiceReportStatus.Overdue);
        var summaryUpcoming = Bucket(r => r.Status == InvoiceReportStatus.Upcoming);
        int totalCount = rows.Count;
        decimal totalAmount = rows.Sum(r => r.Total);

        IEnumerable<InvoiceReportRowDto> scoped = query.Scope switch
        {
            InvoiceReportScope.Vencidas => rows.Where(r => r.Status == InvoiceReportStatus.Overdue),
            InvoiceReportScope.PorVencer => rows.Where(r =>
                r.Status == InvoiceReportStatus.Upcoming
                && (query.DueWithinDays is not int horizon || (r.DaysOverdue is int d && -d <= horizon))),
            InvoiceReportScope.Cobradas => rows.Where(r => r.Status == InvoiceReportStatus.Settled),
            InvoiceReportScope.PorCobrar => rows.Where(r => r.Status != InvoiceReportStatus.Settled),
            _ => rows,
        };

        var ordered = scoped
            .OrderBy(r => r.DueDate is null)
            .ThenBy(r => r.DueDate)
            .ThenBy(r => r.Number, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new InvoiceReportDto(
            DateTime.UtcNow, totalCount, totalAmount,
            summarySettled, summaryPending, summaryOverdue, summaryUpcoming, ordered);
    }
}
