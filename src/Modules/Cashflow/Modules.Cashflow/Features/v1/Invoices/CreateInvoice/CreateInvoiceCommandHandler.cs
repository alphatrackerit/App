using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.CreateInvoice;

public sealed class CreateInvoiceCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<CreateInvoiceCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var invoice = command.Type == InvoiceType.Emitida
            ? Invoice.Issued(
                command.Number!, command.ClientId!.Value, command.Total,
                command.InvoiceDate, command.DueDate, command.CompanyId, command.SocietyId,
                command.TaxBase, command.Vat, command.PaymentTerms, command.StatusId,
                command.DynamicsNumber, command.Notes, command.ProjectId)
            : Invoice.Received(
                command.Number, command.SupplierId!.Value, command.Total,
                command.InvoiceDate, command.DueDate, command.CompanyId,
                command.TaxBase, command.Vat, command.PaymentTerms, command.Bank, command.StatusId,
                command.DynamicsNumber, command.Notes, command.ProjectId);

        if (!string.IsNullOrWhiteSpace(command.DocumentPath))
        {
            invoice.AttachDocument(command.DocumentPath);
        }

        if (command.Items is { Count: > 0 })
        {
            invoice.SetItems(command.Items.Select(i => (i.Description, i.Quantity, i.UnitPrice)));
        }

        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return invoice.Id;
    }
}
