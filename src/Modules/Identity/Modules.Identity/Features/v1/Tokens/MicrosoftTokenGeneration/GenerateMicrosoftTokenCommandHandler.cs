using System.Security.Claims;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Multitenancy;
using FSH.Modules.Auditing.Contracts;
using FSH.Modules.Identity.Authorization.MicrosoftEntra;
using FSH.Modules.Identity.Contracts.DTOs;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Tokens.MicrosoftTokenGeneration;
using FSH.Modules.Identity.Services;
using FSH.Modules.Multitenancy.Contracts;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FSH.Modules.Identity.Features.v1.Tokens.MicrosoftTokenGeneration;

public sealed class GenerateMicrosoftTokenCommandHandler
    : ICommandHandler<GenerateMicrosoftTokenCommand, TokenResponse>
{
    private const string MicrosoftProvider = "Microsoft";

    private readonly IMicrosoftTokenValidator _microsoftTokenValidator;
    private readonly ITenantDomainResolver _tenantDomainResolver;
    private readonly IMultiTenantStore<AppTenantInfo> _tenantStore;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GenerateMicrosoftTokenCommandHandler> _logger;

    public GenerateMicrosoftTokenCommandHandler(
        IMicrosoftTokenValidator microsoftTokenValidator,
        ITenantDomainResolver tenantDomainResolver,
        IMultiTenantStore<AppTenantInfo> tenantStore,
        IServiceScopeFactory scopeFactory,
        ILogger<GenerateMicrosoftTokenCommandHandler> logger)
    {
        _microsoftTokenValidator = microsoftTokenValidator;
        _tenantDomainResolver = tenantDomainResolver;
        _tenantStore = tenantStore;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async ValueTask<TokenResponse> Handle(
        GenerateMicrosoftTokenCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Validate the Microsoft ID token (signature/JWKS, audience, issuer, expiry).
        var principal = await _microsoftTokenValidator
            .ValidateAsync(request.IdToken, cancellationToken)
            .ConfigureAwait(false);

        // 2. Derive the FSH tenant from the verified email domain.
        var domain = ExtractDomain(principal.Email);
        var tenantId = string.IsNullOrEmpty(domain)
            ? null
            : await _tenantDomainResolver.ResolveTenantIdByDomainAsync(domain, cancellationToken).ConfigureAwait(false);

        // Root is operator-only and has no business mapping a Microsoft sign-in domain.
        if (string.IsNullOrEmpty(tenantId)
            || string.Equals(tenantId, MultitenancyConstants.Root.Id, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Microsoft sign-in rejected: no tenant mapped for email domain {Domain}", domain);
            throw new UnauthorizedException(
                $"No tenant is configured for the email domain '{domain}'. Contact your administrator.");
        }

        var tenant = await _tenantStore.GetAsync(tenantId).ConfigureAwait(false);
        if (tenant is null)
        {
            _logger.LogWarning(
                "Microsoft sign-in rejected: tenant {TenantId} mapped from domain {Domain} not found in store", tenantId, domain);
            throw new UnauthorizedException("Tenant not found.");
        }

        // 3. Run the tenant-scoped work in a child scope where the tenant context is established
        //    FIRST, so the Identity DbContext / UserManager bind to the resolved tenant (same
        //    pattern as TenantService / DemoSeeder). Constructor-injecting those services would
        //    capture the unresolved request context instead.
        using var scope = _scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;
        sp.GetRequiredService<IMultiTenantContextSetter>()
            .MultiTenantContext = new MultiTenantContext<AppTenantInfo>(tenant);

        var identityService = sp.GetRequiredService<IIdentityService>();
        var tokenIssuance = sp.GetRequiredService<ITokenIssuanceService>();
        var securityAudit = sp.GetRequiredService<ISecurityAudit>();
        var requestContext = sp.GetRequiredService<IRequestContext>();

        var ip = requestContext.IpAddress ?? "unknown";
        var ua = requestContext.UserAgent ?? "unknown";
        var clientId = requestContext.ClientId ?? "unknown";

        // 4 + 5. Link-only: resolve an existing, active user by email and link the Microsoft identity.
        var identityResult = await identityService
            .ValidateExternalLoginAsync(principal.Email, tenant.Id, MicrosoftProvider, principal.ObjectId, cancellationToken)
            .ConfigureAwait(false);

        if (identityResult is null)
        {
            await securityAudit.LoginFailedAsync(
                subjectIdOrName: principal.Email,
                clientId: clientId,
                reason: "ExternalUserNotLinked",
                ip: ip,
                ct: cancellationToken).ConfigureAwait(false);

            throw new UnauthorizedException(
                $"No active user exists for '{principal.Email}' in tenant '{tenant.Id}'. Ask an administrator to create your account first.");
        }

        var (subject, claims) = identityResult.Value;

        await securityAudit.LoginSucceededAsync(
            userId: subject,
            userName: claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? principal.Email,
            clientId: clientId,
            ip: ip,
            userAgent: ua,
            ct: cancellationToken).ConfigureAwait(false);

        // 6. Issue + persist tokens via the SAME path as password login.
        return await tokenIssuance
            .IssueAndPersistAsync(subject, claims, principal.Email, cancellationToken)
            .ConfigureAwait(false);
    }

    private static string? ExtractDomain(string email)
    {
        var at = email.LastIndexOf('@');
        return at < 0 || at == email.Length - 1 ? null : email[(at + 1)..];
    }
}
