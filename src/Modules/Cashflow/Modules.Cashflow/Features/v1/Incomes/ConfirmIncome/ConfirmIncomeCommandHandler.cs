using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.v1.Incomes;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Incomes.ConfirmIncome;

public sealed class ConfirmIncomeCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<ConfirmIncomeCommand, Unit>
{
    public async ValueTask<Unit> Handle(ConfirmIncomeCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var income = await dbContext.Incomes
            .FirstOrDefaultAsync(i => i.Id == command.IncomeId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Income {command.IncomeId} not found.");

        income.SetConfirmed(command.Confirmed);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
