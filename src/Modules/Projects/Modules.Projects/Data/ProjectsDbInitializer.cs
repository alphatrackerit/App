using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FSH.Modules.Projects.Data;

public sealed class ProjectsDbInitializer(
    ProjectsDbContext dbContext,
    ILogger<ProjectsDbInitializer> logger) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        if ((await dbContext.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("[Projects] applied migrations");
        }
    }

    /// <summary>No per-tenant auto-seed — a fresh tenant comes up with empty project data.</summary>
    public Task SeedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
