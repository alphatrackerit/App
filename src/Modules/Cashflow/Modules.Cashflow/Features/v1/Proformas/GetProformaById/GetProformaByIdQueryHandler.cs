using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.Dtos;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.GetProformaById;

public sealed class GetProformaByIdQueryHandler(CashflowDbContext dbContext)
    : IQueryHandler<GetProformaByIdQuery, ProformaDto>
{
    public async ValueTask<ProformaDto> Handle(GetProformaByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var proforma = await dbContext.Proformas.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == query.ProformaId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Proforma {query.ProformaId} not found.");

        decimal invoiced = await dbContext.Invoices.AsNoTracking()
            .Where(x => x.ProformaId == proforma.Id)
            .SumAsync(x => x.Total, cancellationToken).ConfigureAwait(false);

        return Map(proforma, invoiced);
    }

    /// <summary>Projects a <see cref="Proforma"/> (already materialised) into its DTO. Done in
    /// memory because the <c>PaymentTerms</c> value object is not queryable through its converter.
    /// <paramref name="invoiced"/> is the sum of linked invoice totals (informative cuadre).</summary>
    internal static ProformaDto Map(Proforma p, decimal invoiced) => new(
        p.Id, p.Number, p.Type, p.Date,
        p.ClientId, p.SupplierId, p.CompanyId, p.SocietyId, p.ProjectId, p.TaxBase, p.Vat, p.Total,
        p.PaymentTerms?.Code, p.StatusId, p.Responsible, p.Notes, p.DocumentPath, invoiced);
}
