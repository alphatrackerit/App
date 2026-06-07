using FSH.Modules.Projects.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Projects.Contracts.v1.Notes;

public sealed record GetNoteByIdQuery(Guid NoteId) : IQuery<NoteDto>;
