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
        // generation and manual linking are meant to combine (user decision). The derived-number
        // uniqueness check below is what stops an accidental double-submit.
        var milestones = proforma.PaymentTerms
            .GenerateMilestones(proforma.Total, proforma.Date)
            .ToList();

        var numbers = Enumerable.Range(1, milestones.Count)
            .Select(i => $"{proforma.Number}-{i}")
            .ToList();

        var conflicts = await dbContext.Invoices.AsNoTracking()
            .Where(i => i.Type == proforma.Type && numbers.Contains(i.Number))
            .Select(i => i.Number)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        if (conflicts.Count > 0)
        {
            throw new CustomException(
                $"Invoices with the derived numbers already exist: {string.Join(", ", conflicts)}. " +
                "Rename or delete them first (or the generation already ran).",
                Array.Empty<string>(), HttpStatusCode.Conflict);
        }

        var ids = new List<Guid>();
        for (var i = 0; i < milestones.Count; i++)
        {
            var (amount, dueDate, _) = milestones[i];
            var invoice = proforma.Type == InvoiceType.Emitida
                ? Invoice.Issued(
                    numbers[i], proforma.ClientId!.Value, amount,
                    invoiceDate: proforma.Date, dueDate: dueDate,
                    companyId: proforma.CompanyId, societyId: proforma.SocietyId,
                    notes: null, projectId: proforma.ProjectId)
                : Invoice.Received(
                    numbers[i], proforma.SupplierId!.Value, amount,
                    invoiceDate: proforma.Date, dueDate: dueDate,
                    companyId: proforma.CompanyId,
                    notes: null, projectId: proforma.ProjectId);

            invoice.LinkProforma(proforma.Id);
            dbContext.Invoices.Add(invoice);
            ids.Add(invoice.Id);
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ids;
    }
}
