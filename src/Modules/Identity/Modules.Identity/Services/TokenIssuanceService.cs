using System.Security.Claims;
using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Core.Context;
using FSH.Framework.Eventing.Outbox;
using FSH.Framework.Shared.Multitenancy;
using FSH.Modules.Auditing.Contracts;
using FSH.Modules.Identity.Contracts.DTOs;
using FSH.Modules.Identity.Contracts.Events;
using FSH.Modules.Identity.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace FSH.Modules.Identity.Services;

/// <summary>
/// Shared "authenticated user → FSH tokens" path used by every sign-in flow (password,
/// Microsoft, …). Given a resolved subject + claims it issues the access/refresh pair,
/// persists the refresh token, opens a session, audits the issuance, and enqueues the
/// token-generated integration event. Callers own credential validation and the
/// login-succeeded/failed audit; this owns everything from token issuance onward.
/// </summary>
public interface ITokenIssuanceService
{
    Task<TokenResponse> IssueAndPersistAsync(
        string subject,
        IEnumerable<Claim> claims,
        string emailForAudit,
        CancellationToken ct = default);
}

public sealed class TokenIssuanceService : ITokenIssuanceService
{
    private readonly ITokenService _tokenService;
    private readonly IIdentityService _identityService;
    private readonly ISessionService _sessionService;
    private readonly ISecurityAudit _securityAudit;
    private readonly IRequestContext _requestContext;
    private readonly IOutboxStore _outboxStore;
    private readonly IMultiTenantContextAccessor<AppTenantInfo> _multiTenantContextAccessor;
    private readonly ILogger<TokenIssuanceService> _logger;

    public TokenIssuanceService(
        ITokenService tokenService,
        IIdentityService identityService,
        ISessionService sessionService,
        ISecurityAudit securityAudit,
        IRequestContext requestContext,
        IOutboxStore outboxStore,
        IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor,
        ILogger<TokenIssuanceService> logger)
    {
        _tokenService = tokenService;
        _identityService = identityService;
        _sessionService = sessionService;
        _securityAudit = securityAudit;
        _requestContext = requestContext;
        _outboxStore = outboxStore;
        _multiTenantContextAccessor = multiTenantContextAccessor;
        _logger = logger;
    }

    public async Task<TokenResponse> IssueAndPersistAsync(
        string subject,
        IEnumerable<Claim> claims,
        string emailForAudit,
        CancellationToken ct = default)
    {
        var claimList = claims as IReadOnlyList<Claim> ?? claims.ToList();
        var ip = _requestContext.IpAddress ?? "unknown";
        var ua = _requestContext.UserAgent ?? "unknown";
        var clientId = _requestContext.ClientId;
        var userName = claimList.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? emailForAudit;

        // Issue token
        var token = await _tokenService.IssueAsync(subject, claimList, /*extra*/ null, ct).ConfigureAwait(false);

        // Persist refresh token (hashed) for this user
        await _identityService
            .StoreRefreshTokenAsync(subject, token.RefreshToken, token.RefreshTokenExpiresAt, ct)
            .ConfigureAwait(false);

        // Create user session for session management (non-blocking, fail gracefully)
        try
        {
            var refreshTokenHash = Sha256Short(token.RefreshToken);
            await _sessionService.CreateSessionAsync(
                subject,
                refreshTokenHash,
                ip,
                ua,
                token.RefreshTokenExpiresAt,
                ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Session creation is non-critical — don't fail the login. This can happen if
            // migrations haven't been applied yet.
            _logger.LogWarning(ex, "Failed to create user session for user {UserId}. Login will continue without session tracking.", subject);
        }

        // Audit token issuance with a fingerprint (never raw token)
        var fingerprint = Sha256Short(token.AccessToken);
        await _securityAudit.TokenIssuedAsync(
            userId: subject,
            userName: userName,
            clientId: clientId!,
            tokenFingerprint: fingerprint,
            expiresUtc: token.AccessTokenExpiresAt,
            ct: ct).ConfigureAwait(false);

        // Enqueue integration event for token generation
        var tenantId = _multiTenantContextAccessor.MultiTenantContext?.TenantInfo?.Id;
        var integrationEvent = new TokenGeneratedIntegrationEvent(
            Id: Guid.NewGuid(),
            OccurredOnUtc: TimeProvider.System.GetUtcNow().UtcDateTime,
            TenantId: tenantId,
            CorrelationId: Guid.NewGuid().ToString(),
            Source: "Identity",
            UserId: subject,
            Email: emailForAudit,
            ClientId: clientId!,
            IpAddress: ip,
            UserAgent: ua,
            TokenFingerprint: fingerprint,
            AccessTokenExpiresAtUtc: token.AccessTokenExpiresAt);

        await _outboxStore.AddAsync(integrationEvent, ct).ConfigureAwait(false);

        return token;
    }

    private static string Sha256Short(string value)
    {
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash.AsSpan(0, 8));
    }
}
