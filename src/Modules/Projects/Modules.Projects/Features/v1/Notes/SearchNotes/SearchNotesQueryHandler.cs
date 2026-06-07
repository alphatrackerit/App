using FSH.Framework.Shared.Persistence;
using FSH.Modules.Projects.Contracts.Dtos;
using FSH.Modules.Projects.Contracts.v1.Notes;
using FSH.Modules.Projects.Data;
using FSH.Modules.Projects.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Projects.Features.v1.Notes.SearchNotes;

public sealed class SearchNotesQueryHandler(ProjectsDbContext dbContext)
    : IQueryHandler<SearchNotesQuery, PagedResponse<NoteDto>>
{
    public async ValueTask<PagedResponse<NoteDto>> Handle(SearchNotesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var q = dbContext.Notes.AsNoTracking().AsQueryable();

        if (query.ProjectId.HasValue)
        {
            q = q.Where(n => n.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string term = query.Search.Trim();
            q = q.Where(n =>
                EF.Functions.ILike(n.Title, $"%{term}%") ||
                (n.Description != null && EF.Functions.ILike(n.Description, $"%{term}%")));
        }

        q = ApplySort(q, query.SortBy, query.SortDir);

        long total = await q.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var items = await q
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResponse<NoteDto>
        {
            Items = items
                .Select(n => new NoteDto(n.Id, n.ProjectId, n.Title, n.Description, n.Date))
                .ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size)
        };
    }

    private static IQueryable<Note> ApplySort(IQueryable<Note> q, string? sortBy, string? sortDir)
    {
        bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.ToUpperInvariant()) switch
        {
            "TITLE" => desc ? q.OrderByDescending(n => n.Title) : q.OrderBy(n => n.Title),
            _ => desc ? q.OrderByDescending(n => n.Date) : q.OrderBy(n => n.Date),
        };
    }
}
