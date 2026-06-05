namespace FSH.Modules.Identity.Authorization.MicrosoftEntra;

/// <summary>
/// Configuration for "Sign in with Microsoft" (Entra ID / OIDC) on the dashboard.
/// The backend validates the ID token the SPA obtains via MSAL — it never runs an
/// OIDC redirect/cookie flow itself. Bound from the <c>MicrosoftEntraOptions</c> config
/// section; disabled by default so the feature is opt-in per environment.
/// </summary>
public sealed class MicrosoftEntraOptions
{
    /// <summary>
    /// Master switch. When false the <c>/token/microsoft</c> endpoint rejects every request,
    /// so the feature stays dark until an environment is configured.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// OIDC authority used for metadata / JWKS discovery, e.g.
    /// <c>https://login.microsoftonline.com/organizations/v2.0</c> (any work/school tenant) or
    /// <c>https://login.microsoftonline.com/{entraTenantId}/v2.0</c> (a single organization).
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>The Entra app-registration (SPA) client id — the expected ID-token audience.</summary>
    public string ClientId { get; set; } = string.Empty;
}
