using FSH.Framework.Shared.Persistence;
using FSH.Modules.Cashflow.Contracts.Dtos;
using FSH.Modules.Cashflow.Contracts.v1.Projects;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Projects.SearchProjects;

public sealed class SearchProjectsQueryHandler(CashflowDbContext dbContext)
    : IQueryHandler<SearchProjectsQuery, PagedResponse<ProjectDto>>
{
    public async ValueTask<PagedResponse<ProjectDto>> Handle(SearchProjectsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 10000 ? 20 : query.PageSize;

        var q = dbContext.Projects.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // Every word of the term must match somewhere: the project name OR the name of its
            // client / country / status / company. Word-by-word so "SET ENERGY" matches even with
            // irregular whitespace in the data or terms spread across different columns.
            foreach (string word in query.Search.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                string pattern = $"%{word}%";
                q = q.Where(p =>
                    EF.Functions.ILike(p.Name, pattern) ||
                    dbContext.Clients.Any(c => c.Id == p.ClientId && EF.Functions.ILike(c.Name, pattern)) ||
                    dbContext.Countries.Any(c => c.Id == p.CountryId && EF.Functions.ILike(c.Name, pattern)) ||
                    dbContext.Statuses.Any(s => s.Id == p.StatusId && EF.Functions.ILike(s.Name, pattern)) ||
                    dbContext.Companies.Any(c => c.Id == p.CompanyId && EF.Functions.ILike(c.Name, pattern)));
            }
        }

        if (query.CompanyId is Guid companyId)
        {
            q = q.Where(p => p.CompanyId == companyId);
        }

        q = ApplySort(q, query.SortBy, query.SortDir);

        long total = await q.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var items = await q
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResponse<ProjectDto>
        {
            Items = items
                .Select(p => new ProjectDto(
                    p.Id, p.Name, p.SalePrice, p.ForecastSale, p.Cost, p.ForecastCost, p.Profit,
                    p.ClientId, p.SocietyId, p.CountryId, p.CompanyId, p.StatusId, p.PrefixId))
                .ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        };
    }

    private static IQueryable<Project> ApplySort(IQueryable<Project> q, string? sortBy, string? sortDir)
    {
        bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.ToUpperInvariant()) switch
        {
            "SALEPRICE" => desc ? q.OrderByDescending(p => p.SalePrice) : q.OrderBy(p => p.SalePrice),
            "COST" => desc ? q.OrderByDescending(p => p.Cost) : q.OrderBy(p => p.Cost),
            "PROFIT" => desc ? q.OrderByDescending(p => p.Profit) : q.OrderBy(p => p.Profit),
            _ => desc ? q.OrderByDescending(p => p.Name) : q.OrderBy(p => p.Name),
        };
    }
}
