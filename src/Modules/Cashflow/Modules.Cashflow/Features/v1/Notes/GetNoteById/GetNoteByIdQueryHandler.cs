using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.Dtos;
using FSH.Modules.Cashflow.Contracts.v1.Notes;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Notes.GetNoteById;

public sealed class GetNoteByIdQueryHandler(CashflowDbContext dbContext)
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
