using System.Net;
using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.LinkInvoiceToProforma;

public sealed class LinkInvoiceToProformaCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<LinkInvoiceToProformaCommand, Unit>
{
    public async ValueTask<Unit> Handle(LinkInvoiceToProformaCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var invoice = await dbContext.Invoices
            .FirstOrDefaultAsync(i => i.Id == command.InvoiceId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Invoice {command.InvoiceId} not found.");

        // When linking (not unlinking), the proforma must exist and its type must match.
        if (command.ProformaId is { } proformaId)
        {
            var proforma = await dbContext.Proformas.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == proformaId, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Proforma {proformaId} not found.");

            if (proforma.Type != invoice.Type)
            {
                throw new CustomException(
                    $"A {invoice.Type} invoice can only link to a {invoice.Type} proforma.", Array.Empty<string>(), HttpStatusCode.BadRequest);
            }
        }

        // No amount validation — the cuadre is informative only (user decision).
        invoice.LinkProforma(command.ProformaId);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
