using FSH.Framework.Core.Exceptions;
using FSH.Modules.Projects.Contracts.v1.Incomes;
using FSH.Modules.Projects.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Projects.Features.v1.Incomes.ValidateIncome;

public sealed class ValidateIncomeCommandHandler(ProjectsDbContext dbContext)
    : ICommandHandler<ValidateIncomeCommand, Unit>
{
    public async ValueTask<Unit> Handle(ValidateIncomeCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var income = await dbContext.Incomes
            .FirstOrDefaultAsync(i => i.Id == command.IncomeId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Income {command.IncomeId} not found.");

        income.SetValidated(command.Validated);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
