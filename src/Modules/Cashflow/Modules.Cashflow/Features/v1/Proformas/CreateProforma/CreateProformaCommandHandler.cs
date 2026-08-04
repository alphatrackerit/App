using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.CreateProforma;

public sealed class CreateProformaCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<CreateProformaCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateProformaCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var proforma = command.Type == InvoiceType.Emitida
            ? Proforma.Issued(
                command.Number, command.ClientId!.Value, command.Total,
                command.Date, command.CompanyId, command.SocietyId, command.ProjectId,
                command.TaxBase, command.Vat, command.PaymentTerms, command.StatusId, command.Notes,
                command.Responsible)
            : Proforma.Received(
                command.Number, command.SupplierId!.Value, command.Total,
                command.Date, command.CompanyId, command.ProjectId,
                command.TaxBase, command.Vat, command.PaymentTerms, command.StatusId, command.Notes,
                command.Responsible);

        dbContext.Proformas.Add(proforma);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return proforma.Id;
    }
}
