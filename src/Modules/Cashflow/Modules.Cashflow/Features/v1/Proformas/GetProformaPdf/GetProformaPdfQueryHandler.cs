using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Services;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.GetProformaPdf;

public sealed class GetProformaPdfQueryHandler(CashflowDbContext dbContext, IProformaPdfRenderer renderer)
    : IQueryHandler<GetProformaPdfQuery, ProformaPdfDto>
{
    public async ValueTask<ProformaPdfDto> Handle(GetProformaPdfQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var proforma = await dbContext.Proformas.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.ProformaId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Proforma {query.ProformaId} not found.");

        bool issued = proforma.Type == InvoiceType.Emitida;

        string? counterpartyName = issued
            ? await dbContext.Clients.AsNoTracking()
                .Where(c => c.Id == proforma.ClientId).Select(c => c.Name)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false)
            : await dbContext.Suppliers.AsNoTracking()
                .Where(s => s.Id == proforma.SupplierId).Select(s => s.Name)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        string? companyName = proforma.CompanyId is null
            ? null
            : await dbContext.Companies.AsNoTracking()
                .Where(c => c.Id == proforma.CompanyId).Select(c => c.Name)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        string? statusName = proforma.StatusId is null
            ? null
            : await dbContext.Statuses.AsNoTracking()
                .Where(s => s.Id == proforma.StatusId).Select(s => s.Name)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        var milestones = proforma.PaymentTerms is null
            ? []
            : proforma.PaymentTerms.GenerateMilestones(proforma.Total, proforma.Date).ToList();

        byte[] content = renderer.Render(new ProformaPdfData(
            proforma.Number,
            issued ? "Emitida" : "Recibida",
            proforma.Date,
            issued ? "Cliente" : "Proveedor",
            counterpartyName,
            companyName,
            proforma.TaxBase,
            proforma.Vat,
            proforma.Total,
            proforma.PaymentTerms?.Code,
            statusName,
            proforma.Notes,
            milestones));

        return new ProformaPdfDto(content, $"proforma-{proforma.Number}.pdf");
    }
}
