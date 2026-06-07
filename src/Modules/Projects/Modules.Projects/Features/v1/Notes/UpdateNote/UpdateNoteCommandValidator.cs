using FluentValidation;
using FSH.Modules.Projects.Contracts.v1.Notes;

namespace FSH.Modules.Projects.Features.v1.Notes.UpdateNote;

public sealed class UpdateNoteCommandValidator : AbstractValidator<UpdateNoteCommand>
{
    public UpdateNoteCommandValidator()
    {
        RuleFor(x => x.NoteId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(512);
    }
}
