using FSH.Framework.Shared.Persistence;
using FSH.Modules.Cashflow.Contracts.Dtos;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using FSH.Modules.Cashflow.Features.v1.Invoices.GetInvoiceById;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.SearchInvoices;

public sealed class SearchInvoicesQueryHandler(CashflowDbContext dbContext)
    : IQueryHandler<SearchInvoicesQuery, PagedResponse<InvoiceDto>>
{
    public async ValueTask<PagedResponse<InvoiceDto>> Handle(SearchInvoicesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 10000 ? 20 : query.PageSize;

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

        if (query.ProjectId.HasValue)
        {
            q = q.Where(i => i.ProjectId == query.ProjectId.Value);
        }

        if (query.VerifactuStatus.HasValue)
        {
            q = q.Where(i => i.VerifactuStatus == query.VerifactuStatus.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // Every word of the term must match somewhere: invoice number/Dynamics number, bank,
            // notes, the name of its client / supplier / company / status, or (when the word is
            // numeric) the total / tax base / VAT amounts. Same word-by-word approach as projects.
            foreach (string word in query.Search.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                string pattern = $"%{word}%";
                decimal? amount = decimal.TryParse(word.Replace(',', '.'), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out decimal parsed)
                    ? parsed
                    : null;
                q = q.Where(i =>
                    (i.Number != null && EF.Functions.ILike(i.Number, pattern)) ||
                    (i.DynamicsNumber != null && EF.Functions.ILike(i.DynamicsNumber, pattern)) ||
                    (i.Bank != null && EF.Functions.ILike(i.Bank, pattern)) ||
                    (i.Notes != null && EF.Functions.ILike(i.Notes, pattern)) ||
                    dbContext.Clients.Any(c => c.Id == i.ClientId && EF.Functions.ILike(c.Name, pattern)) ||
                    dbContext.Suppliers.Any(s => s.Id == i.SupplierId && EF.Functions.ILike(s.Name, pattern)) ||
                    dbContext.Companies.Any(c => c.Id == i.CompanyId && EF.Functions.ILike(c.Name, pattern)) ||
                    dbContext.Statuses.Any(s => s.Id == i.StatusId && EF.Functions.ILike(s.Name, pattern)) ||
                    (amount != null && (i.Total == amount || i.TaxBase == amount || i.Vat == amount)));
            }
        }

        q = ApplySort(q, query.SortBy, query.SortDir);

        long total = await q.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var items = await q.Skip((page - 1) * size).Take(size)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        // Cobrado/pagado por factura: suma de líneas validadas vinculadas (misma regla que el reporte).
        var ids = items.Select(i => i.Id).ToList();
        var incomeSums = await dbContext.Incomes.AsNoTracking()
            .Where(x => x.InvoiceId != null && x.Validated && ids.Contains(x.InvoiceId.Value))
            .GroupBy(x => x.InvoiceId!.Value)
            .Select(g => new { g.Key, Sum = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.Key, x => x.Sum, cancellationToken).ConfigureAwait(false);
        var paymentSums = await dbContext.Payments.AsNoTracking()
            .Where(x => x.InvoiceId != null && x.Validated && ids.Contains(x.InvoiceId.Value))
            .GroupBy(x => x.InvoiceId!.Value)
            .Select(g => new { g.Key, Sum = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.Key, x => x.Sum, cancellationToken).ConfigureAwait(false);
        decimal CollectedOf(Invoice i)
        {
            var sums = i.Type == Contracts.Enums.InvoiceType.Emitida ? incomeSums : paymentSums;
            return sums.TryGetValue(i.Id, out decimal s) ? s : 0m;
        }

        return new PagedResponse<InvoiceDto>
        {
            Items = items.Select(i => GetInvoiceByIdQueryHandler.Map(i, CollectedOf(i))).ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size),
        };
    }

    private static IQueryable<Invoice> ApplySort(IQueryable<Invoice> q, string? sortBy, string? sortDir)
    {
        bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        return sortBy?.ToUpperInvariant() switch
        {
            "NUMBER" => desc ? q.OrderByDescending(i => i.Number) : q.OrderBy(i => i.Number),
            "TOTAL" => desc ? q.OrderByDescending(i => i.Total) : q.OrderBy(i => i.Total),
            "DUEDATE" => desc ? q.OrderByDescending(i => i.DueDate) : q.OrderBy(i => i.DueDate),
            "INVOICEDATE" => desc ? q.OrderByDescending(i => i.InvoiceDate) : q.OrderBy(i => i.InvoiceDate),
            _ => q.OrderByDescending(i => i.InvoiceDate),
        };
    }
}
