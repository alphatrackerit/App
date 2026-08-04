using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.AttachProformaDocument;

public sealed class AttachProformaDocumentCommandValidator : AbstractValidator<AttachProformaDocumentCommand>
{
    public AttachProformaDocumentCommandValidator()
    {
        RuleFor(x => x.ProformaId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(128);
    }
}
