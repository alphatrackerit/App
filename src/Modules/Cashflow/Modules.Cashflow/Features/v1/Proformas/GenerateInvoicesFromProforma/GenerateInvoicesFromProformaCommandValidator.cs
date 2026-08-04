using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.GenerateInvoicesFromProforma;

public sealed class GenerateInvoicesFromProformaCommandValidator : AbstractValidator<GenerateInvoicesFromProformaCommand>
{
    public GenerateInvoicesFromProformaCommandValidator()
    {
        RuleFor(x => x.ProformaId).NotEmpty();
    }
}
