using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Incomes;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Incomes.CreateIncome;

public sealed class CreateIncomeCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<CreateIncomeCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateIncomeCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Default to the PENDIENTE (Ingreso) status when none is supplied.
        var statusId = command.StatusId ?? await dbContext.Statuses.AsNoTracking()
            .Where(s => s.Type == StatusType.Ingreso && s.Name == "PENDIENTE")
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var income = Income.Create(
            command.Amount,
            command.Description,
            command.Date,
            command.Percentage,
            command.ProjectId,
            statusId,
            command.Confirmed);

        dbContext.Incomes.Add(income);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return income.Id;
    }
}
