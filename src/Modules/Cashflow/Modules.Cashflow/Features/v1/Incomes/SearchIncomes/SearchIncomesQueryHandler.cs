using FSH.Framework.Shared.Persistence;
using FSH.Modules.Cashflow.Contracts.Dtos;
using FSH.Modules.Cashflow.Contracts.v1.Incomes;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Incomes.SearchIncomes;

public sealed class SearchIncomesQueryHandler(CashflowDbContext dbContext)
    : IQueryHandler<SearchIncomesQuery, PagedResponse<IncomeDto>>
{
    public async ValueTask<PagedResponse<IncomeDto>> Handle(SearchIncomesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 10000 ? 20 : query.PageSize;

        var q = dbContext.Incomes.AsNoTracking().AsQueryable();

        if (query.ProjectId.HasValue)
        {
            q = q.Where(i => i.ProjectId == query.ProjectId.Value);
        }

        if (query.Unlinked == true)
        {
            q = q.Where(i => i.InvoiceId == null);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string term = query.Search.Trim();
            q = q.Where(i => i.Description != null && EF.Functions.ILike(i.Description, $"%{term}%"));
        }

        q = ApplySort(q, query.SortBy, query.SortDir);

        long total = await q.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var items = await q
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResponse<IncomeDto>
        {
            Items = items
                .Select(i => new IncomeDto(
                    i.Id, i.Amount, i.Description, i.Date, i.Percentage, i.ProjectId, i.StatusId, i.Confirmed, i.Validated, i.InvoiceId))
                .ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        };
    }

    private static IQueryable<Income> ApplySort(IQueryable<Income> q, string? sortBy, string? sortDir)
    {
        bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.ToUpperInvariant()) switch
        {
            "AMOUNT" => desc ? q.OrderByDescending(i => i.Amount) : q.OrderBy(i => i.Amount),
            "PERCENTAGE" => desc ? q.OrderByDescending(i => i.Percentage) : q.OrderBy(i => i.Percentage),
            _ => desc ? q.OrderByDescending(i => i.Date) : q.OrderBy(i => i.Date),
        };
    }
}
