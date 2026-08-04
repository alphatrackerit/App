using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.DeleteProforma;

public sealed class DeleteProformaCommandValidator : AbstractValidator<DeleteProformaCommand>
{
    public DeleteProformaCommandValidator()
    {
        RuleFor(x => x.ProformaId).NotEmpty();
    }
}
