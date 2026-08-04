using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.Dtos;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.GetProformaLines;

public sealed class GetProformaLinesQueryHandler(CashflowDbContext dbContext)
    : IQueryHandler<GetProformaLinesQuery, ProformaLinesDto>
{
    public async ValueTask<ProformaLinesDto> Handle(GetProformaLinesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var proforma = await dbContext.Proformas.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.ProformaId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Proforma {query.ProformaId} not found.");

        var invoices = await dbContext.Invoices.AsNoTracking()
            .Where(i => i.ProformaId == proforma.Id)
            .OrderBy(i => i.InvoiceDate)
            .Select(i => new ProformaInvoiceDto(i.Id, i.Number, i.InvoiceDate, i.DueDate, i.Total, i.StatusId))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        decimal invoiced = invoices.Sum(i => i.Total);

        // Pending may be negative (over-invoiced) — informative only, never blocks.
        return new ProformaLinesDto(proforma.Id, proforma.Total, invoiced, proforma.Total - invoiced, invoices);
    }
}
