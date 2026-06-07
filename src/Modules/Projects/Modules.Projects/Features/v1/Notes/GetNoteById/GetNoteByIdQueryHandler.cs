using FSH.Framework.Core.Exceptions;
using FSH.Modules.Projects.Contracts.Dtos;
using FSH.Modules.Projects.Contracts.v1.Notes;
using FSH.Modules.Projects.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Projects.Features.v1.Notes.GetNoteById;

public sealed class GetNoteByIdQueryHandler(ProjectsDbContext dbContext)
    : IQueryHandler<GetNoteByIdQuery, NoteDto>
{
    public async ValueTask<NoteDto> Handle(GetNoteByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var n = await dbContext.Notes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == query.NoteId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Note {query.NoteId} not found.");

        return new NoteDto(n.Id, n.ProjectId, n.Title, n.Description, n.Date);
    }
}
