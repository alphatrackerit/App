using System.Net;
using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.LinkLineToInvoice;

public sealed class LinkLineToInvoiceCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<LinkLineToInvoiceCommand, Unit>
{
    public async ValueTask<Unit> Handle(LinkLineToInvoiceCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // When linking (not unlinking), the invoice must exist and its type must match the line kind.
        if (command.InvoiceId is { } invoiceId)
        {
            var invoice = await dbContext.Invoices.AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Invoice {invoiceId} not found.");

            var expected = command.Kind == CashLineKind.Income ? InvoiceType.Emitida : InvoiceType.Recibida;
            if (invoice.Type != expected)
            {
                throw new CustomException(
                    $"A {command.Kind} line can only link to an {expected} invoice.", Array.Empty<string>(), HttpStatusCode.BadRequest);
            }
        }

        if (command.Kind == CashLineKind.Income)
        {
            var income = await dbContext.Incomes
                .FirstOrDefaultAsync(x => x.Id == command.LineId, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Income {command.LineId} not found.");
            income.LinkInvoice(command.InvoiceId);
        }
        else
        {
            var payment = await dbContext.Payments
                .FirstOrDefaultAsync(x => x.Id == command.LineId, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Payment {command.LineId} not found.");
            payment.LinkInvoice(command.InvoiceId);
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
