using System.Net;
using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.GenerateInvoicesFromProforma;

public sealed class GenerateInvoicesFromProformaCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<GenerateInvoicesFromProformaCommand, IReadOnlyList<Guid>>
{
    public async ValueTask<IReadOnlyList<Guid>> Handle(GenerateInvoicesFromProformaCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var proforma = await dbContext.Proformas
            .FirstOrDefaultAsync(p => p.Id == command.ProformaId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Proforma {command.ProformaId} not found.");

        if (proforma.PaymentTerms is null)
        {
            throw new CustomException(
                "The proforma has no payment terms (FormaPago) to generate invoices from.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        // Unlike invoice milestone generation, invoices already linked do NOT block — automatic
        // generation and manual linking are meant to combine (user decision).
        var milestones = proforma.PaymentTerms
            .GenerateMilestones(proforma.Total, proforma.Date)
            .ToList();

        if (proforma.Type == InvoiceType.Emitida)
        {
            // Emitidas keep the derived numbering; the uniqueness check below is what stops an
            // accidental double-submit.
            var numbers = Enumerable.Range(1, milestones.Count)
                .Select(i => $"{proforma.Number}-{i}")
                .ToList();

            var conflicts = await dbContext.Invoices.AsNoTracking()
                .Where(i => i.Type == proforma.Type && i.Number != null && numbers.Contains(i.Number))
                .Select(i => i.Number)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            if (conflicts.Count > 0)
            {
                throw new CustomException(
                    $"Ya existen facturas con los números derivados: {string.Join(", ", conflicts)}. " +
                    "Renómbralas o bórralas primero (o la generación ya se ejecutó).",
                    Array.Empty<string>(), HttpStatusCode.Conflict);
            }
        }
        else
        {
            // Recibidas are created WITHOUT a number (draft until the supplier's invoice arrives),
            // so the derived-number guard cannot apply. Refuse instead when a full set of linked
            // invoices already exists — the likely double-submit.
            int linked = await dbContext.Invoices.AsNoTracking()
                .CountAsync(i => i.ProformaId == proforma.Id, cancellationToken).ConfigureAwait(false);
            if (linked >= milestones.Count)
            {
                throw new CustomException(
                    "La proforma ya tiene facturas generadas o vinculadas para todos sus plazos; " +
                    "desvincúlalas o bórralas primero para regenerar.",
                    Array.Empty<string>(), HttpStatusCode.Conflict);
            }
        }

        var ids = new List<Guid>();
        decimal taxBaseAllocated = 0m, vatAllocated = 0m;
        for (var i = 0; i < milestones.Count; i++)
        {
            var (amount, dueDate, _) = milestones[i];

            // Prorate the proforma's tax base / VAT by each instalment's share of the total; the
            // last instalment absorbs the rounding remainder so the parts re-sum exactly.
            bool isLast = i == milestones.Count - 1;
            decimal ratio = proforma.Total != 0m ? amount / proforma.Total : 0m;
            decimal? taxBase = null, vat = null;
            if (proforma.TaxBase is { } totalBase)
            {
                taxBase = isLast ? totalBase - taxBaseAllocated : Math.Round(totalBase * ratio, 2, MidpointRounding.AwayFromZero);
                taxBaseAllocated += taxBase.Value;
            }

            if (proforma.Vat is { } totalVat)
            {
                vat = isLast ? totalVat - vatAllocated : Math.Round(totalVat * ratio, 2, MidpointRounding.AwayFromZero);
                vatAllocated += vat.Value;
            }

            // Each generated invoice covers ONE instalment, so it inherits that instalment's terms
            // (100PP / ND) — inheriting the proforma's full code would re-split each invoice again.
            var milestoneTerms = proforma.PaymentTerms.Milestones[i].Trigger == PaymentTrigger.Prepaid
                ? "100PP"
                : $"{proforma.PaymentTerms.Milestones[i].OffsetDays}D";

            var invoice = proforma.Type == InvoiceType.Emitida
                ? Invoice.Issued(
                    $"{proforma.Number}-{i + 1}", proforma.ClientId!.Value, amount,
                    invoiceDate: proforma.Date, dueDate: dueDate,
                    companyId: proforma.CompanyId, societyId: proforma.SocietyId,
                    taxBase: taxBase, vat: vat,
                    paymentTerms: milestoneTerms,
                    notes: null, projectId: proforma.ProjectId)
                : Invoice.Received(
                    number: null, supplierId: proforma.SupplierId!.Value, total: amount,
                    invoiceDate: proforma.Date, dueDate: dueDate,
                    companyId: proforma.CompanyId,
                    taxBase: taxBase, vat: vat,
                    paymentTerms: milestoneTerms,
                    notes: null, projectId: proforma.ProjectId);

            invoice.LinkProforma(proforma.Id);
            dbContext.Invoices.Add(invoice);
            ids.Add(invoice.Id);
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ids;
    }
}
