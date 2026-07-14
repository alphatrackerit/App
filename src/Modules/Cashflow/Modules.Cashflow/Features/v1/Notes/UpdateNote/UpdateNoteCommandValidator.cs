using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Notes;

namespace FSH.Modules.Cashflow.Features.v1.Notes.UpdateNote;

public sealed class UpdateNoteCommandValidator : AbstractValidator<UpdateNoteCommand>
{
    public UpdateNoteCommandValidator()
    {
        RuleFor(x => x.NoteId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(512);
    }
}
