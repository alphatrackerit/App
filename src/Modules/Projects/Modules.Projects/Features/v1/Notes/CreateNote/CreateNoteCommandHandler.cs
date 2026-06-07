using FSH.Modules.Projects.Contracts.v1.Notes;
using FSH.Modules.Projects.Data;
using FSH.Modules.Projects.Domain;
using Mediator;

namespace FSH.Modules.Projects.Features.v1.Notes.CreateNote;

public sealed class CreateNoteCommandHandler(ProjectsDbContext dbContext)
    : ICommandHandler<CreateNoteCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateNoteCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var note = Note.Create(command.ProjectId, command.Title, command.Description, command.Date);

        dbContext.Notes.Add(note);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return note.Id;
    }
}
