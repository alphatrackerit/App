using FSH.Framework.Shared.Persistence;
using FSH.Modules.Cashflow.Contracts.Dtos;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using FSH.Modules.Cashflow.Features.v1.Proformas.GetProformaById;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.SearchProformas;

public sealed class SearchProformasQueryHandler(CashflowDbContext dbContext)
    : IQueryHandler<SearchProformasQuery, PagedResponse<ProformaDto>>
{
    public async ValueTask<PagedResponse<ProformaDto>> Handle(SearchProformasQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 10000 ? 20 : query.PageSize;

        var q = dbContext.Proformas.AsNoTracking().AsQueryable();

        if (query.Type.HasValue)
        {
            q = q.Where(p => p.Type == query.Type.Value);
        }

        if (query.ClientId.HasValue)
        {
            q = q.Where(p => p.ClientId == query.ClientId.Value);
        }

        if (query.SupplierId.HasValue)
        {
            q = q.Where(p => p.SupplierId == query.SupplierId.Value);
        }

        if (query.CompanyId.HasValue)
        {
            q = q.Where(p => p.CompanyId == query.CompanyId.Value);
        }

        if (query.ProjectId.HasValue)
        {
            q = q.Where(p => p.ProjectId == query.ProjectId.Value);
        }

        // Pending-work filters run BEFORE count/pagination so TotalCount reflects them.
        q = query.Pending switch
        {
            ProformaPendingFilter.SinFactura =>
                q.Where(p => !dbContext.Invoices.Any(i => i.ProformaId == p.Id)),
            ProformaPendingFilter.FacturasSinNumero =>
                q.Where(p => dbContext.Invoices.Any(i => i.ProformaId == p.Id && i.Number == null)),
            ProformaPendingFilter.ParcialmenteFacturada =>
                q.Where(p => dbContext.Invoices.Any(i => i.ProformaId == p.Id)
                    && dbContext.Invoices.Where(i => i.ProformaId == p.Id).Sum(i => i.Total) < p.Total),
            _ => q,
        };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // Word-by-word matching, same approach as invoices/projects.
            foreach (string word in query.Search.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                string pattern = $"%{word}%";
                decimal? amount = decimal.TryParse(word.Replace(',', '.'), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out decimal parsed)
                    ? parsed
                    : null;
                q = q.Where(p =>
                    EF.Functions.ILike(p.Number, pattern) ||
                    (p.Notes != null && EF.Functions.ILike(p.Notes, pattern)) ||
                    (p.Responsible != null && EF.Functions.ILike(p.Responsible, pattern)) ||
                    dbContext.Clients.Any(c => c.Id == p.ClientId && EF.Functions.ILike(c.Name, pattern)) ||
                    dbContext.Suppliers.Any(s => s.Id == p.SupplierId && EF.Functions.ILike(s.Name, pattern)) ||
                    dbContext.Companies.Any(c => c.Id == p.CompanyId && EF.Functions.ILike(c.Name, pattern)) ||
                    dbContext.Statuses.Any(s => s.Id == p.StatusId && EF.Functions.ILike(s.Name, pattern)) ||
                    (amount != null && (p.Total == amount || p.TaxBase == amount || p.Vat == amount)));
            }
        }

        q = ApplySort(q, query.SortBy, query.SortDir);

        long total = await q.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var items = await q.Skip((page - 1) * size).Take(size)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        // Invoiced amount per proforma — informative cuadre, same derivation everywhere.
        var ids = items.Select(p => p.Id).ToList();
        var invoicedSums = await dbContext.Invoices.AsNoTracking()
            .Where(i => i.ProformaId != null && ids.Contains(i.ProformaId.Value))
            .GroupBy(i => i.ProformaId!.Value)
            .Select(g => new { g.Key, Sum = g.Sum(i => i.Total) })
            .ToDictionaryAsync(x => x.Key, x => x.Sum, cancellationToken).ConfigureAwait(false);

        return new PagedResponse<ProformaDto>
        {
            Items = items
                .Select(p => GetProformaByIdQueryHandler.Map(
                    p, invoicedSums.TryGetValue(p.Id, out decimal s) ? s : 0m))
                .ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size),
        };
    }

    private static IQueryable<Proforma> ApplySort(IQueryable<Proforma> q, string? sortBy, string? sortDir)
    {
        bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        return sortBy?.ToUpperInvariant() switch
        {
            "NUMBER" => desc ? q.OrderByDescending(p => p.Number) : q.OrderBy(p => p.Number),
            "TOTAL" => desc ? q.OrderByDescending(p => p.Total) : q.OrderBy(p => p.Total),
            "DATE" => desc ? q.OrderByDescending(p => p.Date) : q.OrderBy(p => p.Date),
            _ => q.OrderByDescending(p => p.Date),
        };
    }
}
