using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.DeleteProforma;

public sealed class DeleteProformaCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<DeleteProformaCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteProformaCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var proforma = await dbContext.Proformas
            .FirstOrDefaultAsync(x => x.Id == command.ProformaId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Proforma {command.ProformaId} not found.");

        // ON DELETE SET NULL (config) unlinks any invoices — they survive as standalone documents.
        dbContext.Proformas.Remove(proforma);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
