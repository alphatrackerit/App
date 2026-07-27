using System.Net;
using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.GenerateMilestones;

public sealed class GenerateMilestonesCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<GenerateMilestonesCommand, IReadOnlyList<Guid>>
{
    public async ValueTask<IReadOnlyList<Guid>> Handle(GenerateMilestonesCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var invoice = await dbContext.Invoices
            .FirstOrDefaultAsync(i => i.Id == command.InvoiceId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Invoice {command.InvoiceId} not found.");

        if (invoice.PaymentTerms is null)
        {
            throw new CustomException(
                "The invoice has no payment terms (FormaPago) to generate milestones from.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        // Idempotency guard: refuse if this invoice already has generated/linked lines (§7.6).
        bool alreadyLinked = invoice.Type == InvoiceType.Emitida
            ? await dbContext.Incomes.AnyAsync(x => x.InvoiceId == invoice.Id, cancellationToken).ConfigureAwait(false)
            : await dbContext.Payments.AnyAsync(x => x.InvoiceId == invoice.Id, cancellationToken).ConfigureAwait(false);
        if (alreadyLinked)
        {
            throw new CustomException(
                "The invoice already has generated lines; unlink them first to regenerate.", Array.Empty<string>(), HttpStatusCode.Conflict);
        }

        var milestones = invoice.PaymentTerms.GenerateMilestones(invoice.Total, invoice.InvoiceDate ?? invoice.DueDate);
        var ids = new List<Guid>();

        foreach (var (amount, dueDate, percentage) in milestones)
        {
            var date = dueDate ?? invoice.InvoiceDate;
            if (invoice.Type == InvoiceType.Emitida)
            {
                var income = Income.Create(
                    amount, invoice.Number, date, percentage, command.ProjectId, statusId: null, confirmed: false);
                income.LinkInvoice(invoice.Id);
                dbContext.Incomes.Add(income);
                ids.Add(income.Id);
            }
            else
            {
                var payment = Payment.Create(
                    amount, invoice.Number, date, percentage, invoice.SupplierId, command.ProjectId, statusId: null, confirmed: false);
                payment.LinkInvoice(invoice.Id);
                dbContext.Payments.Add(payment);
                ids.Add(payment.Id);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ids;
    }
}
