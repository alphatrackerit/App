using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.v1.Projects;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Projects.DeleteProject;

public sealed class DeleteProjectCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<DeleteProjectCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteProjectCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var project = await dbContext.Projects
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Project {command.ProjectId} not found.");

        dbContext.Projects.Remove(project);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
