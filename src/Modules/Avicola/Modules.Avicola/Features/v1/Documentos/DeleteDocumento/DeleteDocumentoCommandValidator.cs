using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Documentos;

namespace FSH.Modules.Avicola.Features.v1.Documentos.DeleteDocumento;

public sealed class DeleteDocumentoCommandValidator : AbstractValidator<DeleteDocumentoCommand>
{
    public DeleteDocumentoCommandValidator()
    {
        RuleFor(x => x.DocumentoId).NotEmpty();
    }
}
