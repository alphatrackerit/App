using FSH.Modules.Cashflow.Contracts.v1.Incomes;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;

namespace FSH.Modules.Cashflow.Features.v1.Incomes.CreateIncome;

public sealed class CreateIncomeCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<CreateIncomeCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateIncomeCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var income = Income.Create(
            command.Amount,
            command.Description,
            command.Date,
            command.Percentage,
            command.ProjectId,
            command.StatusId,
            command.Confirmed);

        dbContext.Incomes.Add(income);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return income.Id;
    }
}
