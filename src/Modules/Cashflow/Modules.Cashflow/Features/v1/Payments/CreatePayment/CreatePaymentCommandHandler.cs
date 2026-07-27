using System.Net;
using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Payments;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Payments.CreatePayment;

public sealed class CreatePaymentCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<CreatePaymentCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreatePaymentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Linking at creation follows the same rules as LinkLineToInvoice: the invoice
        // must exist and a payment can only hang from a Recibida invoice.
        if (command.InvoiceId is { } invoiceId)
        {
            var invoiceType = await dbContext.Invoices.AsNoTracking()
                .Where(i => i.Id == invoiceId)
                .Select(i => (InvoiceType?)i.Type)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Invoice {invoiceId} not found.");

            if (invoiceType != InvoiceType.Recibida)
            {
                throw new CustomException(
                    "A Payment line can only link to a Recibida invoice.", Array.Empty<string>(), HttpStatusCode.BadRequest);
            }
        }

        // Default to the PENDIENTE (Pago) status when none is supplied.
        var statusId = command.StatusId ?? await dbContext.Statuses.AsNoTracking()
            .Where(s => s.Type == StatusType.Pago && s.Name == "PENDIENTE")
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var payment = Payment.Create(
            command.Amount,
            command.Description,
            command.Date,
            command.Percentage,
            command.SupplierId,
            command.ProjectId,
            statusId,
            command.Confirmed);
        payment.LinkInvoice(command.InvoiceId);

        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return payment.Id;
    }
}
