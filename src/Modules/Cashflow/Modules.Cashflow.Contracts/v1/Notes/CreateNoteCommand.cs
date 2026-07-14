using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Notes;

public sealed record CreateNoteCommand(
    string Title,
    Guid? ProjectId = null,
    string? Description = null,
    DateTimeOffset? Date = null) : ICommand<Guid>;
