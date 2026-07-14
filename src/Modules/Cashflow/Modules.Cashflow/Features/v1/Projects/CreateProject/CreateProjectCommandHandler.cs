using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Projects;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Projects.CreateProject;

public sealed class CreateProjectCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<CreateProjectCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateProjectCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Default to the PREVISTO (Proyecto) status when none is supplied.
        var statusId = command.StatusId ?? await dbContext.Statuses.AsNoTracking()
            .Where(s => s.Type == StatusType.Proyecto && s.Name == "PREVISTO")
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var project = Project.Create(
            command.Name,
            command.SalePrice,
            command.ForecastSale,
            command.Cost,
            command.ForecastCost,
            command.Profit,
            command.ClientId,
            command.SocietyId,
            command.CountryId,
            command.CompanyId,
            statusId,
            command.PrefixId);

        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return project.Id;
    }
}
