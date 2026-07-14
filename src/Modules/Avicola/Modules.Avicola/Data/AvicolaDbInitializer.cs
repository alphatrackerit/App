using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FSH.Modules.Avicola.Data;

public sealed class AvicolaDbInitializer(
    AvicolaDbContext dbContext,
    ILogger<AvicolaDbInitializer> logger) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        if ((await dbContext.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("[Avicola] applied migrations");
        }
    }

    /// <summary>No per-tenant auto-seed — a fresh tenant comes up with empty poultry data.</summary>
    public Task SeedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
