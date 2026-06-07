using Mediator;

namespace FSH.Modules.Projects.Contracts.v1.Notes;

public sealed record DeleteNoteCommand(Guid NoteId) : ICommand<Unit>;
