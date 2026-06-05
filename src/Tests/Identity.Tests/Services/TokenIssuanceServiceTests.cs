using AutoFixture;
using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Core.Context;
using FSH.Framework.Eventing.Abstractions;
using FSH.Framework.Eventing.Outbox;
using FSH.Framework.Shared.Multitenancy;
using FSH.Modules.Auditing.Contracts;
using FSH.Modules.Identity.Contracts.DTOs;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Security.Claims;

namespace Identity.Tests.Services;

/// <summary>
/// Tests for TokenIssuanceService — the shared "authenticated user → FSH tokens" path used by
/// both password and Microsoft sign-in. Covers token issuance, refresh-token persistence, session
/// creation (incl. graceful failure), token-issued audit, and the integration-event enqueue.
/// </summary>
public sealed class TokenIssuanceServiceTests
{
    private readonly ITokenService _tokenService;
    private readonly IIdentityService _identityService;
    private readonly ISessionService _sessionService;
    private readonly ISecurityAudit _securityAudit;
    private readonly IRequestContext _requestContext;
    private readonly IOutboxStore _outboxStore;
    private readonly IMultiTenantContextAccessor<AppTenantInfo> _multiTenantContextAccessor;
    private readonly TokenIssuanceService _sut;
    private readonly IFixture _fixture;

    public TokenIssuanceServiceTests()
    {
        _tokenService = Substitute.For<ITokenService>();
        _identityService = Substitute.For<IIdentityService>();
        _sessionService = Substitute.For<ISessionService>();
        _securityAudit = Substitute.For<ISecurityAudit>();
        _requestContext = Substitute.For<IRequestContext>();
        _outboxStore = Substitute.For<IOutboxStore>();
        _multiTenantContextAccessor = Substitute.For<IMultiTenantContextAccessor<AppTenantInfo>>();

        _requestContext.IpAddress.Returns("192.168.1.1");
        _requestContext.UserAgent.Returns("TestAgent");
        _requestContext.ClientId.Returns("test-client");

        _sut = new TokenIssuanceService(
            _tokenService,
            _identityService,
            _sessionService,
            _securityAudit,
            _requestContext,
            _outboxStore,
            _multiTenantContextAccessor,
            Substitute.For<ILogger<TokenIssuanceService>>());

        _fixture = new Fixture();
    }

    [Fact]
    public async Task IssueAndPersist_Should_IssuePersistSessionAuditAndEnqueueEvent()
    {
        var userId = _fixture.Create<string>();
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId), new(ClaimTypes.Name, "TestUser") };
        var token = _fixture.Create<TokenResponse>();
        _tokenService.IssueAsync(userId, Arg.Any<IEnumerable<Claim>>(), null, Arg.Any<CancellationToken>()).Returns(token);

        var result = await _sut.IssueAndPersistAsync(userId, claims, "user@example.com", CancellationToken.None);

        result.ShouldBe(token);
        await _identityService.Received(1).StoreRefreshTokenAsync(userId, token.RefreshToken, token.RefreshTokenExpiresAt, Arg.Any<CancellationToken>());
        await _sessionService.Received(1).CreateSessionAsync(userId, Arg.Any<string>(), "192.168.1.1", "TestAgent", token.RefreshTokenExpiresAt, Arg.Any<CancellationToken>());
        await _securityAudit.Received(1).TokenIssuedAsync(userId, Arg.Any<string>(), "test-client", Arg.Any<string>(), token.AccessTokenExpiresAt, Arg.Any<CancellationToken>());
        await _outboxStore.Received(1).AddAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IssueAndPersist_Should_ReturnToken_When_SessionCreationFails()
    {
        var userId = _fixture.Create<string>();
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        var token = _fixture.Create<TokenResponse>();
        _tokenService.IssueAsync(userId, Arg.Any<IEnumerable<Claim>>(), null, Arg.Any<CancellationToken>()).Returns(token);
        _sessionService.CreateSessionAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Database not available"));

        var result = await _sut.IssueAndPersistAsync(userId, claims, "user@example.com", CancellationToken.None);

        result.ShouldBe(token);
        await _outboxStore.Received(1).AddAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
    }
}
