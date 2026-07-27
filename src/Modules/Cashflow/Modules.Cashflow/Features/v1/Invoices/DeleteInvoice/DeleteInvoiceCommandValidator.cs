using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.DeleteInvoice;

public sealed class DeleteInvoiceCommandValidator : AbstractValidator<DeleteInvoiceCommand>
{
    public DeleteInvoiceCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
    }
}
