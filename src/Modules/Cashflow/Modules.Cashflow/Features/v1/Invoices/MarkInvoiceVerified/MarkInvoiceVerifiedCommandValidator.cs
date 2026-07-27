using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.MarkInvoiceVerified;

public sealed class MarkInvoiceVerifiedCommandValidator : AbstractValidator<MarkInvoiceVerifiedCommand>
{
    public MarkInvoiceVerifiedCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
    }
}
