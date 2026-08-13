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

        // Los mensajes llegan tal cual al usuario (el dashboard los muestra en el toast).
        RuleFor(x => x.ClientId).NotNull()
            .When(x => x.Type == InvoiceType.Emitida)
            .WithMessage("Una proforma emitida requiere un cliente.");

        RuleFor(x => x.SupplierId).NotNull()
            .When(x => x.Type == InvoiceType.Recibida)
            .WithMessage("Una proforma recibida requiere un proveedor.");

        RuleFor(x => x.PaymentTerms!)
            .Must(pt => PaymentTerms.TryParse(pt, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.PaymentTerms))
            .WithMessage("Forma de pago no reconocida (p. ej. 60D, 100PP, 30PP70-60D, DOMICILIADO).");
    }
}
