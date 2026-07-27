using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.ApplyLinkSuggestions;

public sealed class ApplyLinkSuggestionsCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<ApplyLinkSuggestionsCommand, ApplyLinkSuggestionsResult>
{
    public async ValueTask<ApplyLinkSuggestionsResult> Handle(ApplyLinkSuggestionsCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Re-validate every pair against current state: suggestions may be stale by the time the
        // user accepts them. Invalid pairs are skipped (counted), never fatal — one bad pair must
        // not abort a 500-pair batch.
        var invoiceIds = command.Pairs.Select(p => p.InvoiceId).Distinct().ToList();
        var invoiceTypes = await dbContext.Invoices.AsNoTracking()
            .Where(i => invoiceIds.Contains(i.Id))
            .Select(i => new { i.Id, i.Type })
            .ToDictionaryAsync(i => i.Id, i => i.Type, cancellationToken)
            .ConfigureAwait(false);

        var incomeIds = command.Pairs.Where(p => p.Kind == CashLineKind.Income).Select(p => p.LineId).Distinct().ToList();
        var paymentIds = command.Pairs.Where(p => p.Kind == CashLineKind.Payment).Select(p => p.LineId).Distinct().ToList();

        var incomes = await dbContext.Incomes
            .Where(l => incomeIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, cancellationToken)
            .ConfigureAwait(false);
        var payments = await dbContext.Payments
            .Where(l => paymentIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, cancellationToken)
            .ConfigureAwait(false);

        int linked = 0, skipped = 0;
        foreach (var pair in command.Pairs)
        {
            if (!invoiceTypes.TryGetValue(pair.InvoiceId, out var type))
            {
                skipped++;
                continue;
            }

            var expected = pair.Kind == CashLineKind.Income ? InvoiceType.Emitida : InvoiceType.Recibida;
            if (type != expected)
            {
                skipped++;
                continue;
            }

            if (pair.Kind == CashLineKind.Income)
            {
                if (incomes.TryGetValue(pair.LineId, out var income) && income.InvoiceId is null)
                {
                    income.LinkInvoice(pair.InvoiceId);
                    linked++;
                }
                else
                {
                    skipped++;
                }
            }
            else
            {
                if (payments.TryGetValue(pair.LineId, out var payment) && payment.InvoiceId is null)
                {
                    payment.LinkInvoice(pair.InvoiceId);
                    linked++;
                }
                else
                {
                    skipped++;
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new ApplyLinkSuggestionsResult(linked, skipped);
    }
}
