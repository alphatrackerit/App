using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Documentos;

namespace FSH.Modules.Avicola.Features.v1.Documentos.UpdateDocumento;

public sealed class UpdateDocumentoCommandValidator : AbstractValidator<UpdateDocumentoCommand>
{
    public UpdateDocumentoCommandValidator()
    {
        RuleFor(x => x.DocumentoId).NotEmpty();
    }
}
