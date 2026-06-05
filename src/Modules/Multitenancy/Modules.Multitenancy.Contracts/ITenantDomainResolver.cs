namespace FSH.Modules.Multitenancy.Contracts;

/// <summary>
/// Resolves the FSH tenant that owns a given email domain. Used by external sign-in
/// (e.g. Microsoft / Entra ID) to pick the tenant from the authenticated user's email
/// domain before any tenant context exists. Returns <c>null</c> when no tenant claims
/// the domain.
/// </summary>
public interface ITenantDomainResolver
{
    Task<string?> ResolveTenantIdByDomainAsync(string emailDomain, CancellationToken cancellationToken = default);
}
