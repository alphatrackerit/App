using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Notes;

namespace FSH.Modules.Cashflow.Features.v1.Notes.CreateNote;

public sealed class CreateNoteCommandValidator : AbstractValidator<CreateNoteCommand>
{
    public CreateNoteCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(512);
    }
}
