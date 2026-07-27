using System.Net;
using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Incomes;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Incomes.CreateIncome;

public sealed class CreateIncomeCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<CreateIncomeCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateIncomeCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Linking at creation follows the same rules as LinkLineToInvoice: the invoice
        // must exist and an income can only hang from an Emitida invoice.
        if (command.InvoiceId is { } invoiceId)
        {
            var invoiceType = await dbContext.Invoices.AsNoTracking()
                .Where(i => i.Id == invoiceId)
                .Select(i => (InvoiceType?)i.Type)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Invoice {invoiceId} not found.");

            if (invoiceType != InvoiceType.Emitida)
            {
                throw new CustomException(
                    "An Income line can only link to an Emitida invoice.", Array.Empty<string>(), HttpStatusCode.BadRequest);
            }
        }

        // Default to the PENDIENTE (Ingreso) status when none is supplied.
        var statusId = command.StatusId ?? await dbContext.Statuses.AsNoTracking()
            .Where(s => s.Type == StatusType.Ingreso && s.Name == "PENDIENTE")
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var income = Income.Create(
            command.Amount,
            command.Description,
            command.Date,
            command.Percentage,
            command.ProjectId,
            statusId,
            command.Confirmed);
        income.LinkInvoice(command.InvoiceId);

        dbContext.Incomes.Add(income);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return income.Id;
    }
}
