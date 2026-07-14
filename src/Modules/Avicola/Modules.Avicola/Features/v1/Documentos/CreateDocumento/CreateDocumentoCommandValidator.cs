using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Documentos;

namespace FSH.Modules.Avicola.Features.v1.Documentos.CreateDocumento;

public sealed class CreateDocumentoCommandValidator : AbstractValidator<CreateDocumentoCommand>
{
    public CreateDocumentoCommandValidator()
    {
        RuleFor(x => x.Url).NotEmpty().MaximumLength(2048);
        RuleFor(x => x.NombreArchivo).NotEmpty().MaximumLength(512);
    }
}
