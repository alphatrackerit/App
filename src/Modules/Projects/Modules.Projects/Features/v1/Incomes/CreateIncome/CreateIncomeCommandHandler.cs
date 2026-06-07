using FSH.Modules.Projects.Contracts.v1.Incomes;
using FSH.Modules.Projects.Data;
using FSH.Modules.Projects.Domain;
using Mediator;

namespace FSH.Modules.Projects.Features.v1.Incomes.CreateIncome;

public sealed class CreateIncomeCommandHandler(ProjectsDbContext dbContext)
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
