using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.LinkInvoiceToProforma;

public sealed class LinkInvoiceToProformaCommandValidator : AbstractValidator<LinkInvoiceToProformaCommand>
{
    public LinkInvoiceToProformaCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
    }
}
