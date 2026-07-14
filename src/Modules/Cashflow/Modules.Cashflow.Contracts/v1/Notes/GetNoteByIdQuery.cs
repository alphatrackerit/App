using FSH.Modules.Cashflow.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Notes;

public sealed record GetNoteByIdQuery(Guid NoteId) : IQuery<NoteDto>;
