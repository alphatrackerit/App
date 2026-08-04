using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using FSH.Modules.Cashflow.Domain;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.UpdateInvoice;

public sealed class UpdateInvoiceCommandValidator : AbstractValidator<UpdateInvoiceCommand>
{
    public UpdateInvoiceCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.Number).NotEmpty().MaximumLength(64);

        RuleFor(x => x.PaymentTerms!)
            .Must(pt => PaymentTerms.TryParse(pt, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.PaymentTerms))
            .WithMessage("Unrecognized payment-terms code.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Description).NotEmpty().MaximumLength(512);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        }).When(x => x.Items is not null);
    }
}
