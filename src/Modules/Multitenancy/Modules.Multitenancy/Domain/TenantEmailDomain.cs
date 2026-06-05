namespace FSH.Modules.Multitenancy.Domain;

/// <summary>
/// Maps a verified email domain (e.g. <c>acme.com</c>) to the FSH tenant that owns it.
/// Lives in the global tenant catalog (<see cref="Data.TenantDbContext"/>, which is NOT
/// tenant-filtered) so it can be resolved during external sign-in (Microsoft / Entra ID)
/// BEFORE any tenant context exists — the authenticated user's email domain is what
/// selects the tenant. A domain maps to exactly one tenant (enforced by a unique index).
/// </summary>
public sealed class TenantEmailDomain
{
    public Guid Id { get; private set; }
    public string Domain { get; private set; } = default!;
    public string TenantId { get; private set; } = default!;

    private TenantEmailDomain() { }

    public static TenantEmailDomain Create(string domain, string tenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        return new TenantEmailDomain
        {
            Id = Guid.CreateVersion7(),
            Domain = Normalize(domain),
            TenantId = tenantId.Trim(),
        };
    }

    /// <summary>Canonical lowercase form used for both storage and lookups.</summary>
    public static string Normalize(string domain)
    {
        ArgumentNullException.ThrowIfNull(domain);
#pragma warning disable CA1308 // domains are canonical lowercase, not security-sensitive
        return domain.Trim().ToLowerInvariant();
#pragma warning restore CA1308
    }
}
