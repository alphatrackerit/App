using System.Collections.Concurrent;
using FSH.Framework.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace FSH.Modules.Identity.Authorization.MicrosoftEntra;

/// <summary>
/// Validates Microsoft (Entra ID) ID tokens offline against the provider's published JWKS.
/// Registered as a singleton so the OIDC metadata + signing keys are fetched once and cached
/// (with automatic refresh) by <see cref="ConfigurationManager{T}"/> rather than per request.
/// </summary>
public sealed class MicrosoftTokenValidator : IMicrosoftTokenValidator
{
    private readonly IOptionsMonitor<MicrosoftEntraOptions> _options;
    private readonly ILogger<MicrosoftTokenValidator> _logger;

    // One config manager per authority (authorities rarely change; bounded set).
    private readonly ConcurrentDictionary<string, ConfigurationManager<OpenIdConnectConfiguration>> _configManagers =
        new(StringComparer.Ordinal);

    public MicrosoftTokenValidator(
        IOptionsMonitor<MicrosoftEntraOptions> options,
        ILogger<MicrosoftTokenValidator> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<MicrosoftPrincipal> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idToken);

        var opts = _options.CurrentValue;
        if (!opts.Enabled)
        {
            throw new UnauthorizedException("Microsoft sign-in is not enabled.");
        }
        if (string.IsNullOrWhiteSpace(opts.Authority) || string.IsNullOrWhiteSpace(opts.ClientId))
        {
            throw new UnauthorizedException("Microsoft sign-in is misconfigured.");
        }

        var configManager = _configManagers.GetOrAdd(opts.Authority, static authority =>
            new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{authority.TrimEnd('/')}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever()));

        OpenIdConnectConfiguration config;
        try
        {
            config = await configManager.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Entra OIDC metadata from {Authority}", opts.Authority);
            throw new UnauthorizedException("Unable to verify Microsoft sign-in at this time.");
        }

        var parameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = opts.ClientId,
            ValidateIssuer = true,
            // Entra is multi-tenant: the issuer carries the user's home tenant id, so accept any
            // well-formed Microsoft v2.0 issuer rather than pinning a single one.
            IssuerValidator = ValidateEntraIssuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = config.SigningKeys,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),
        };

        var handler = new JsonWebTokenHandler();
        var result = await handler.ValidateTokenAsync(idToken, parameters).ConfigureAwait(false);
        if (!result.IsValid)
        {
            _logger.LogWarning(result.Exception, "Microsoft ID token validation failed.");
            throw new UnauthorizedException("Microsoft sign-in failed.");
        }

        var claims = result.ClaimsIdentity;
        var email = claims.FindFirst("email")?.Value
            ?? claims.FindFirst("preferred_username")?.Value;
        var objectId = claims.FindFirst("oid")?.Value
            ?? claims.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        var name = claims.FindFirst("name")?.Value;

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new UnauthorizedException("Microsoft account did not provide a verified email.");
        }
        if (string.IsNullOrWhiteSpace(objectId))
        {
            throw new UnauthorizedException("Microsoft account did not provide an object id.");
        }

        return new MicrosoftPrincipal(email.Trim(), objectId, name);
    }

    private static string ValidateEntraIssuer(string issuer, SecurityToken securityToken, TokenValidationParameters validationParameters)
    {
        // Accept Entra v2.0 issuers (https://login.microsoftonline.com/{tenantId}/v2.0)
        // and v1.0 issuers (https://sts.windows.net/{tenantId}/). The audience + signature
        // checks above are what actually bind the token to our app.
        if (issuer.StartsWith("https://login.microsoftonline.com/", StringComparison.OrdinalIgnoreCase)
            && issuer.EndsWith("/v2.0", StringComparison.OrdinalIgnoreCase))
        {
            return issuer;
        }
        if (issuer.StartsWith("https://sts.windows.net/", StringComparison.OrdinalIgnoreCase))
        {
            return issuer;
        }

        throw new SecurityTokenInvalidIssuerException($"Issuer '{issuer}' is not a recognized Microsoft Entra issuer.");
    }
}
