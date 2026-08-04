using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Verifactu;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.IssueInvoiceVerifactu;

public sealed class IssueInvoiceVerifactuCommandValidator : AbstractValidator<IssueInvoiceVerifactuCommand>
{
    public IssueInvoiceVerifactuCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
    }
}
