using System.Security.Claims;
using FSH.Framework.Core.Context;
using FSH.Modules.Auditing.Contracts;
using FSH.Modules.Identity.Contracts.DTOs;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Tokens.TokenGeneration;
using FSH.Modules.Identity.Services;
using Mediator;

namespace FSH.Modules.Identity.Features.v1.Tokens.TokenGeneration;

public sealed class GenerateTokenCommandHandler
    : ICommandHandler<GenerateTokenCommand, TokenResponse>
{
    private readonly IIdentityService _identityService;
    private readonly ISecurityAudit _securityAudit;
    private readonly IRequestContext _requestContext;
    private readonly ITokenIssuanceService _tokenIssuance;

    public GenerateTokenCommandHandler(
        IIdentityService identityService,
        ISecurityAudit securityAudit,
        IRequestContext requestContext,
        ITokenIssuanceService tokenIssuance)
    {
        _identityService = identityService;
        _securityAudit = securityAudit;
        _requestContext = requestContext;
        _tokenIssuance = tokenIssuance;
    }

    public async ValueTask<TokenResponse> Handle(
        GenerateTokenCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var ip = _requestContext.IpAddress ?? "unknown";
        var ua = _requestContext.UserAgent ?? "unknown";
        var clientId = _requestContext.ClientId;

        // Validate credentials (includes 2FA verification when the user has it enabled)
        var identityResult = await _identityService
            .ValidateCredentialsAsync(request.Email, request.Password, request.TwoFactorCode, cancellationToken)
            .ConfigureAwait(false);

        if (identityResult is null)
        {
            await _securityAudit.LoginFailedAsync(
                subjectIdOrName: request.Email,
                clientId: clientId!,
                reason: "InvalidCredentials",
                ip: ip,
                ct: cancellationToken).ConfigureAwait(false);

            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var (subject, claims) = identityResult.Value;

        await _securityAudit.LoginSucceededAsync(
            userId: subject,
            userName: claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? request.Email,
            clientId: clientId!,
            ip: ip,
            userAgent: ua,
            ct: cancellationToken).ConfigureAwait(false);

        // Issue + persist tokens via the shared path (session, audit, integration event).
        return await _tokenIssuance
            .IssueAndPersistAsync(subject, claims, request.Email, cancellationToken)
            .ConfigureAwait(false);
    }
}
