using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;
using FSH.Modules.Cashflow.Domain;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.UpdateProforma;

public sealed class UpdateProformaCommandValidator : AbstractValidator<UpdateProformaCommand>
{
    public UpdateProformaCommandValidator()
    {
        RuleFor(x => x.ProformaId).NotEmpty();
        RuleFor(x => x.Number).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Responsible).MaximumLength(128);

        RuleFor(x => x.PaymentTerms!)
            .Must(pt => PaymentTerms.TryParse(pt, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.PaymentTerms))
            .WithMessage("Unrecognized payment-terms code.");
    }
}
