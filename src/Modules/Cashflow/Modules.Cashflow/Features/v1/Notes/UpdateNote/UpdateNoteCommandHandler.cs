using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.v1.Notes;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Notes.UpdateNote;

public sealed class UpdateNoteCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<UpdateNoteCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdateNoteCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var note = await dbContext.Notes
            .FirstOrDefaultAsync(n => n.Id == command.NoteId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Note {command.NoteId} not found.");

        note.Update(command.ProjectId, command.Title, command.Description, command.Date);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return note.Id;
    }
}
