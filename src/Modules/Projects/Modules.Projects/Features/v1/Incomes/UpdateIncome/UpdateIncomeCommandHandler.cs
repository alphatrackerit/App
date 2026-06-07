using FSH.Framework.Core.Exceptions;
using FSH.Modules.Projects.Contracts.v1.Incomes;
using FSH.Modules.Projects.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Projects.Features.v1.Incomes.UpdateIncome;

public sealed class UpdateIncomeCommandHandler(ProjectsDbContext dbContext)
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
