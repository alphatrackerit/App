using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.LinkLineToInvoice;

public sealed class LinkLineToInvoiceCommandValidator : AbstractValidator<LinkLineToInvoiceCommand>
{
    public LinkLineToInvoiceCommandValidator()
    {
        RuleFor(x => x.LineId).NotEmpty();
        RuleFor(x => x.Kind).IsInEnum();
    }
}
