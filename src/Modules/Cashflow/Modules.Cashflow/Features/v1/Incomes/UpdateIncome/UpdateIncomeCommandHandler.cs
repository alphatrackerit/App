using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.v1.Incomes;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Incomes.UpdateIncome;

public sealed class UpdateIncomeCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<UpdateIncomeCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdateIncomeCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var income = await dbContext.Incomes
            .FirstOrDefaultAsync(i => i.Id == command.IncomeId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Income {command.IncomeId} not found.");

        income.Update(
            command.Amount,
            command.Description,
            command.Date,
            command.Percentage,
            command.ProjectId,
            command.StatusId,
            command.Confirmed);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return income.Id;
    }
}
