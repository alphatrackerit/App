using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Notes;

public sealed record DeleteNoteCommand(Guid NoteId) : ICommand<Unit>;
