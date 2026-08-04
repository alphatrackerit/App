using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.Dtos;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.GetInvoiceById;

public sealed class GetInvoiceByIdQueryHandler(CashflowDbContext dbContext)
    : IQueryHandler<GetInvoiceByIdQuery, InvoiceDto>
{
    public async ValueTask<InvoiceDto> Handle(GetInvoiceByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var invoice = await dbContext.Invoices.AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == query.InvoiceId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Invoice {query.InvoiceId} not found.");

        decimal collected = invoice.Type == InvoiceType.Emitida
            ? await dbContext.Incomes.AsNoTracking()
                .Where(x => x.InvoiceId == invoice.Id && x.Validated)
                .SumAsync(x => x.Amount, cancellationToken).ConfigureAwait(false)
            : await dbContext.Payments.AsNoTracking()
                .Where(x => x.InvoiceId == invoice.Id && x.Validated)
                .SumAsync(x => x.Amount, cancellationToken).ConfigureAwait(false);

        return Map(invoice, collected);
    }

    /// <summary>Projects an <see cref="Invoice"/> (already materialised) into its DTO. Done in memory
    /// because the <c>PaymentTerms</c> value object is not queryable through its converter.
    /// <paramref name="collected"/> is the sum of validated linked lines (same rule as the report).
    /// Items come from the loaded navigation (empty when the caller didn't Include them — search).</summary>
    internal static InvoiceDto Map(Invoice i, decimal collected) => new(
        i.Id, i.Number, i.DynamicsNumber, i.Type, i.InvoiceDate, i.DueDate,
        i.ClientId, i.SupplierId, i.CompanyId, i.SocietyId, i.ProjectId, i.TaxBase, i.Vat, i.Total,
        i.PaymentTerms?.Code, i.Bank, i.StatusId, i.Verified, i.Notes, i.DocumentPath, collected,
        i.ProformaId, i.VerifactuStatus,
        i.Items.OrderBy(x => x.Position)
            .Select(x => new InvoiceItemDto(x.Id, x.Description, x.Quantity, x.UnitPrice, x.Amount))
            .ToList());
}
