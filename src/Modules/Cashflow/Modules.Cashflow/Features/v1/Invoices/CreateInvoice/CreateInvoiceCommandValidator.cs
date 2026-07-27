using FluentValidation;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using FSH.Modules.Cashflow.Domain;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.CreateInvoice;

public sealed class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.Number).NotEmpty().MaximumLength(64);

        RuleFor(x => x.ClientId).NotNull()
            .When(x => x.Type == InvoiceType.Emitida)
            .WithMessage("An issued (Emitida) invoice requires a ClientId.");

        RuleFor(x => x.SupplierId).NotNull()
            .When(x => x.Type == InvoiceType.Recibida)
            .WithMessage("A received (Recibida) invoice requires a SupplierId.");

        RuleFor(x => x.PaymentTerms!)
            .Must(pt => PaymentTerms.TryParse(pt, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.PaymentTerms))
            .WithMessage("Unrecognized payment-terms code.");
    }
}
