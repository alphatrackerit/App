using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.UpdateProforma;

public sealed class UpdateProformaCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<UpdateProformaCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdateProformaCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var proforma = await dbContext.Proformas
            .FirstOrDefaultAsync(x => x.Id == command.ProformaId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Proforma {command.ProformaId} not found.");

        proforma.Update(
            command.Number, command.Total, command.Date,
            command.ClientId, command.SupplierId, command.CompanyId, command.SocietyId,
            command.ProjectId, command.TaxBase, command.Vat, command.PaymentTerms,
            command.StatusId, command.Notes, command.Responsible);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return proforma.Id;
    }
}
