using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Galpones;

namespace FSH.Modules.Avicola.Features.v1.Galpones.DeleteGalpon;

public sealed class DeleteGalponCommandValidator : AbstractValidator<DeleteGalponCommand>
{
    public DeleteGalponCommandValidator()
    {
        RuleFor(x => x.GalponId).NotEmpty();
    }
}
