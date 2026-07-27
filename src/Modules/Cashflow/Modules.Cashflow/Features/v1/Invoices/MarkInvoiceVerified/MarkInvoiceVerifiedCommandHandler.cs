using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.MarkInvoiceVerified;

public sealed class MarkInvoiceVerifiedCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<MarkInvoiceVerifiedCommand, Unit>
{
    public async ValueTask<Unit> Handle(MarkInvoiceVerifiedCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var invoice = await dbContext.Invoices
            .FirstOrDefaultAsync(i => i.Id == command.InvoiceId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Invoice {command.InvoiceId} not found.");

        invoice.MarkVerified();
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
