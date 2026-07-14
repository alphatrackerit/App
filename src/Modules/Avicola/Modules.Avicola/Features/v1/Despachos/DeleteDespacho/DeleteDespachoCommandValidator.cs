using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Despachos;

namespace FSH.Modules.Avicola.Features.v1.Despachos.DeleteDespacho;

public sealed class DeleteDespachoCommandValidator : AbstractValidator<DeleteDespachoCommand>
{
    public DeleteDespachoCommandValidator()
    {
        RuleFor(x => x.DespachoId).NotEmpty();
    }
}
