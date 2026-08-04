using FSH.Framework.Persistence;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FSH.Modules.Cashflow.Data;

public sealed class CashflowDbInitializer(
    CashflowDbContext dbContext,
    ILogger<CashflowDbInitializer> logger) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        if ((await dbContext.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("[Cashflow] applied migrations");
        }
    }

    /// <summary>
    /// Seeds the minimum data a fresh tenant needs to operate: the typed status catalog
    /// (per <see cref="StatusType"/>) plus at least one Country and one Company so projects
    /// can be created. Idempotent — safe to re-run.
    /// </summary>
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await SeedStatusesAsync(cancellationToken).ConfigureAwait(false);
        await SeedCountriesAsync(cancellationToken).ConfigureAwait(false);
        await SeedCompanyAsync(cancellationToken).ConfigureAwait(false);
    }

    private static readonly (StatusType Type, string Name)[] DefaultStatuses =
    [
        (StatusType.Proyecto, "PREVISTO"),
        (StatusType.Proyecto, "EN CURSO"),
        (StatusType.Proyecto, "CERRADO"),
        (StatusType.Ingreso, "PENDIENTE"),
        (StatusType.Ingreso, "CONFIRMADO"),
        (StatusType.Pago, "PENDIENTE"),
        (StatusType.Pago, "CONFIRMADO"),
        (StatusType.FacturaEmitida, "BORRADOR"),
        (StatusType.FacturaEmitida, "EMITIDA"),
        (StatusType.FacturaEmitida, "COBRADA PARCIAL"),
        (StatusType.FacturaEmitida, "COBRADA"),
        (StatusType.FacturaEmitida, "VENCIDA"),
        (StatusType.FacturaRecibida, "PENDIENTE"),
        (StatusType.FacturaRecibida, "PAGADA PARCIAL"),
        (StatusType.FacturaRecibida, "PAGADA"),
        (StatusType.ProformaEmitida, "BORRADOR"),
        (StatusType.ProformaEmitida, "ENVIADA"),
        (StatusType.ProformaEmitida, "ACEPTADA"),
        (StatusType.ProformaEmitida, "RECHAZADA"),
        (StatusType.ProformaEmitida, "FACTURADA PARCIAL"),
        (StatusType.ProformaEmitida, "FACTURADA COMPLETA"),
        (StatusType.ProformaEmitida, "CANCELADA"),
        (StatusType.ProformaRecibida, "BORRADOR"),
        (StatusType.ProformaRecibida, "ENVIADA"),
        (StatusType.ProformaRecibida, "ACEPTADA"),
        (StatusType.ProformaRecibida, "RECHAZADA"),
        (StatusType.ProformaRecibida, "FACTURADA PARCIAL"),
        (StatusType.ProformaRecibida, "FACTURADA COMPLETA"),
        (StatusType.ProformaRecibida, "CANCELADA"),
    ];

    private async Task SeedStatusesAsync(CancellationToken cancellationToken)
    {
        var existing = (await dbContext.Statuses.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false))
            .Where(s => s.Type is not null)
            .Select(s => (s.Type!.Value, s.Name))
            .ToHashSet();

        bool added = false;
        foreach (var (type, name) in DefaultStatuses)
        {
            if (existing.Add((type, name)))
            {
                dbContext.Statuses.Add(Status.Create(name, null, type, null));
                added = true;
            }
        }

        if (added)
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("[Cashflow] seeded default statuses");
        }
    }

    private async Task SeedCountriesAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.Countries.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        dbContext.Countries.Add(Country.Create("España", "ES", null));
        dbContext.Countries.Add(Country.Create("Chile", "CL", null));
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        logger.LogInformation("[Cashflow] seeded default countries");
    }

    private async Task SeedCompanyAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.Companies.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        dbContext.Companies.Add(Company.Create("Empresa Principal", null, null, null));
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        logger.LogInformation("[Cashflow] seeded default company");
    }
}
