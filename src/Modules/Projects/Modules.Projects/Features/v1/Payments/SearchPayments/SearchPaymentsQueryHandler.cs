using FSH.Framework.Shared.Persistence;
using FSH.Modules.Projects.Contracts.Dtos;
using FSH.Modules.Projects.Contracts.v1.Payments;
using FSH.Modules.Projects.Data;
using FSH.Modules.Projects.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Projects.Features.v1.Payments.SearchPayments;

public sealed class SearchPaymentsQueryHandler(ProjectsDbContext dbContext)
    : IQueryHandler<SearchPaymentsQuery, PagedResponse<PaymentDto>>
{
    public async ValueTask<PagedResponse<PaymentDto>> Handle(SearchPaymentsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var q = dbContext.Payments.AsNoTracking().AsQueryable();

        if (query.ProjectId.HasValue)
        {
            q = q.Where(p => p.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string term = query.Search.Trim();
            q = q.Where(p => p.Description != null && EF.Functions.ILike(p.Description, $"%{term}%"));
        }

        q = ApplySort(q, query.SortBy, query.SortDir);

        long total = await q.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var items = await q
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResponse<PaymentDto>
        {
            Items = items
                .Select(p => new PaymentDto(
                    p.Id, p.Amount, p.Description, p.Date, p.Percentage, p.SupplierId, p.ProjectId, p.StatusId, p.Confirmed, p.Validated))
                .ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        };
    }

    private static IQueryable<Payment> ApplySort(IQueryable<Payment> q, string? sortBy, string? sortDir)
    {
        bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.ToUpperInvariant()) switch
        {
            "AMOUNT" => desc ? q.OrderByDescending(p => p.Amount) : q.OrderBy(p => p.Amount),
            "PERCENTAGE" => desc ? q.OrderByDescending(p => p.Percentage) : q.OrderBy(p => p.Percentage),
            _ => desc ? q.OrderByDescending(p => p.Date) : q.OrderBy(p => p.Date),
        };
    }
}
