using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.DeleteInvoice;

public sealed class DeleteInvoiceCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<DeleteInvoiceCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteInvoiceCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var invoice = await dbContext.Invoices
            .FirstOrDefaultAsync(i => i.Id == command.InvoiceId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Invoice {command.InvoiceId} not found.");

        // A VeriFactu-registered invoice is immutable (409) — deleting it would break the AEAT chain.
        invoice.EnsureNotVerifactuRegistered();

        // ON DELETE SET NULL (config) unlinks any Income/Payment lines back to forecast state.
        dbContext.Invoices.Remove(invoice);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
