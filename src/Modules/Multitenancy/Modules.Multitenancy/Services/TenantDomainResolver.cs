using FSH.Modules.Multitenancy.Contracts;
using FSH.Modules.Multitenancy.Data;
using FSH.Modules.Multitenancy.Domain;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Multitenancy.Services;

/// <summary>
/// Resolves an email domain to its owning tenant by querying the global tenant catalog
/// (<see cref="TenantDbContext"/>). Lives in the Multitenancy module; exposed to other
/// modules through <see cref="ITenantDomainResolver"/> in the Contracts assembly.
/// </summary>
public sealed class TenantDomainResolver : ITenantDomainResolver
{
    private readonly TenantDbContext _dbContext;

    public TenantDomainResolver(TenantDbContext dbContext) => _dbContext = dbContext;

    public async Task<string?> ResolveTenantIdByDomainAsync(string emailDomain, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(emailDomain))
        {
            return null;
        }

        var normalized = TenantEmailDomain.Normalize(emailDomain);

        return await _dbContext.TenantEmailDomains
            .Where(d => d.Domain == normalized)
            .Select(d => d.TenantId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
