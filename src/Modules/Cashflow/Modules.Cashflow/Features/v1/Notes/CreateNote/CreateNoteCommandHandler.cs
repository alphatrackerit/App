using FSH.Modules.Cashflow.Contracts.v1.Notes;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;

namespace FSH.Modules.Cashflow.Features.v1.Notes.CreateNote;

public sealed class CreateNoteCommandHandler(CashflowDbContext dbContext)
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
