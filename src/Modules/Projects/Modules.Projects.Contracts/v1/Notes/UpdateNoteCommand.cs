using Mediator;

namespace FSH.Modules.Projects.Contracts.v1.Notes;

public sealed record UpdateNoteCommand(
    Guid NoteId,
    string Title,
    Guid? ProjectId = null,
    string? Description = null,
    DateTimeOffset? Date = null) : ICommand<Guid>;
