using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.Dtos;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.GetInvoiceLines;

public sealed class GetInvoiceLinesQueryHandler(CashflowDbContext dbContext)
    : IQueryHandler<GetInvoiceLinesQuery, InvoiceLinesDto>
{
    public async ValueTask<InvoiceLinesDto> Handle(GetInvoiceLinesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var invoice = await dbContext.Invoices.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == query.InvoiceId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Invoice {query.InvoiceId} not found.");

        List<InvoiceLineDto> lines = invoice.Type == InvoiceType.Emitida
            ? await dbContext.Incomes.AsNoTracking()
                .Where(x => x.InvoiceId == invoice.Id)
                .OrderBy(x => x.Date)
                .Select(x => new InvoiceLineDto(
                    x.Id, CashLineKind.Income, x.Amount, x.Date, x.Percentage, x.Description, x.ProjectId, x.StatusId, null, x.Confirmed, x.Validated))
                .ToListAsync(cancellationToken).ConfigureAwait(false)
            : await dbContext.Payments.AsNoTracking()
                .Where(x => x.InvoiceId == invoice.Id)
                .OrderBy(x => x.Date)
                .Select(x => new InvoiceLineDto(
                    x.Id, CashLineKind.Payment, x.Amount, x.Date, x.Percentage, x.Description, x.ProjectId, x.StatusId, x.SupplierId, x.Confirmed, x.Validated))
                .ToListAsync(cancellationToken).ConfigureAwait(false);

        decimal linked = lines.Sum(l => l.Amount);
        decimal validated = lines.Where(l => l.Validated).Sum(l => l.Amount);

        return new InvoiceLinesDto(invoice.Id, invoice.Total, linked, validated, invoice.Total - validated, lines);
    }
}
