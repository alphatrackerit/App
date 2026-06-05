namespace FSH.Modules.Identity.Authorization.MicrosoftEntra;

/// <summary>The verified identity extracted from a Microsoft (Entra ID) ID token.</summary>
/// <param name="Email">Verified email / UPN of the signed-in Microsoft account.</param>
/// <param name="ObjectId">Stable Entra object id (<c>oid</c>) — the durable external-identity key.</param>
/// <param name="Name">Display name, when present.</param>
public sealed record MicrosoftPrincipal(string Email, string ObjectId, string? Name);

public interface IMicrosoftTokenValidator
{
    /// <summary>
    /// Validates an Entra-issued ID token (signature via JWKS, audience == ClientId, issuer, expiry)
    /// and returns the verified principal. Throws when the feature is disabled/misconfigured, the
    /// token is invalid, or the token carries no verified email / object id.
    /// </summary>
    Task<MicrosoftPrincipal> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}
