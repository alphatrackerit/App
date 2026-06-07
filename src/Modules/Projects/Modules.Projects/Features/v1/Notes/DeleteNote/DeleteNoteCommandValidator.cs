using FluentValidation;
using FSH.Modules.Projects.Contracts.v1.Notes;

namespace FSH.Modules.Projects.Features.v1.Notes.DeleteNote;

public sealed class DeleteNoteCommandValidator : AbstractValidator<DeleteNoteCommand>
{
    public DeleteNoteCommandValidator()
    {
        RuleFor(x => x.NoteId).NotEmpty();
    }
}
