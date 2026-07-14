using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Notes;

namespace FSH.Modules.Cashflow.Features.v1.Notes.DeleteNote;

public sealed class DeleteNoteCommandValidator : AbstractValidator<DeleteNoteCommand>
{
    public DeleteNoteCommandValidator()
    {
        RuleFor(x => x.NoteId).NotEmpty();
    }
}
