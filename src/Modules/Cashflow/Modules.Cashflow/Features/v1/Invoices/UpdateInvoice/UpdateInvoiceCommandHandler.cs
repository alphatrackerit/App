using System.Net;
using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.UpdateInvoice;

public sealed class UpdateInvoiceCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<UpdateInvoiceCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdateInvoiceCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var invoice = await dbContext.Invoices
            .FirstOrDefaultAsync(i => i.Id == command.InvoiceId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Invoice {command.InvoiceId} not found.");

        // The counterparty side is fixed by the (immutable) invoice Type — guard here so a bad edit
        // returns a clean 400 instead of a DB check-constraint 500.
        if (invoice.Type == InvoiceType.Emitida && command.ClientId is null)
        {
            throw new CustomException("An issued (Emitida) invoice requires a ClientId.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        if (invoice.Type == InvoiceType.Recibida && command.SupplierId is null)
        {
            throw new CustomException("A received (Recibida) invoice requires a SupplierId.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        invoice.Update(
            command.Number, command.Total, command.InvoiceDate, command.DueDate,
            command.ClientId, command.SupplierId, command.CompanyId, command.SocietyId,
            command.TaxBase, command.Vat, command.PaymentTerms, command.Bank, command.StatusId,
            command.DynamicsNumber, command.Notes, command.ProjectId);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return invoice.Id;
    }
}
