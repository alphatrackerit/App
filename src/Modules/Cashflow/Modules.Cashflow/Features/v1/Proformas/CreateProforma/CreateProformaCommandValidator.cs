using FluentValidation;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;
using FSH.Modules.Cashflow.Domain;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.CreateProforma;

public sealed class CreateProformaCommandValidator : AbstractValidator<CreateProformaCommand>
{
    public CreateProformaCommandValidator()
    {
        RuleFor(x => x.Number).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Responsible).MaximumLength(128);

        RuleFor(x => x.ClientId).NotNull()
            .When(x => x.Type == InvoiceType.Emitida)
            .WithMessage("An issued (Emitida) proforma requires a ClientId.");

        RuleFor(x => x.SupplierId).NotNull()
            .When(x => x.Type == InvoiceType.Recibida)
            .WithMessage("A received (Recibida) proforma requires a SupplierId.");

        RuleFor(x => x.PaymentTerms!)
            .Must(pt => PaymentTerms.TryParse(pt, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.PaymentTerms))
            .WithMessage("Unrecognized payment-terms code.");
    }
}
