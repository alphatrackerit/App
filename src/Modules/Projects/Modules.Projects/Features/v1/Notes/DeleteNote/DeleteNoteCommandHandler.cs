using FSH.Framework.Core.Exceptions;
using FSH.Modules.Projects.Contracts.v1.Notes;
using FSH.Modules.Projects.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Projects.Features.v1.Notes.DeleteNote;

public sealed class DeleteNoteCommandHandler(ProjectsDbContext dbContext)
    : ICommandHandler<DeleteNoteCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteNoteCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var note = await dbContext.Notes
            .FirstOrDefaultAsync(n => n.Id == command.NoteId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Note {command.NoteId} not found.");

        dbContext.Notes.Remove(note);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
