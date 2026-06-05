using AutoFixture;
using FSH.Framework.Core.Context;
using FSH.Modules.Auditing.Contracts;
using FSH.Modules.Identity.Contracts.DTOs;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Tokens.TokenGeneration;
using FSH.Modules.Identity.Features.v1.Tokens.TokenGeneration;
using FSH.Modules.Identity.Services;
using NSubstitute;
using System.Security.Claims;

namespace Identity.Tests.Handlers;

/// <summary>
/// Tests for GenerateTokenCommandHandler — credential validation, login auditing, and delegation
/// to the shared <see cref="ITokenIssuanceService"/>. Token issuance internals (refresh-token
/// persistence, session creation, outbox event) are covered by <c>TokenIssuanceServiceTests</c>.
/// </summary>
public sealed class GenerateTokenCommandHandlerTests
{
    private readonly IIdentityService _identityService;
    private readonly ISecurityAudit _securityAudit;
    private readonly IRequestContext _requestContext;
    private readonly ITokenIssuanceService _tokenIssuance;
    private readonly GenerateTokenCommandHandler _sut;
    private readonly IFixture _fixture;

    public GenerateTokenCommandHandlerTests()
    {
        _identityService = Substitute.For<IIdentityService>();
        _securityAudit = Substitute.For<ISecurityAudit>();
        _requestContext = Substitute.For<IRequestContext>();
        _tokenIssuance = Substitute.For<ITokenIssuanceService>();

        _sut = new GenerateTokenCommandHandler(
            _identityService,
            _securityAudit,
            _requestContext,
            _tokenIssuance);

        _fixture = new Fixture();
    }

    [Fact]
    public async Task Handle_Should_ReturnIssuedToken_When_CredentialsAreValid()
    {
        var command = new GenerateTokenCommand("user@example.com", "password123");
        var userId = _fixture.Create<string>();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, "TestUser"),
            new(ClaimTypes.Email, command.Email),
        };
        var expectedToken = _fixture.Create<TokenResponse>();

        _requestContext.IpAddress.Returns("192.168.1.1");
        _requestContext.UserAgent.Returns("TestAgent");
        _requestContext.ClientId.Returns("test-client");

        _identityService.ValidateCredentialsAsync(command.Email, command.Password, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((userId, claims));
        _tokenIssuance.IssueAndPersistAsync(userId, Arg.Any<IEnumerable<Claim>>(), command.Email, Arg.Any<CancellationToken>())
            .Returns(expectedToken);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.ShouldBe(expectedToken);
    }

    [Fact]
    public async Task Handle_Should_ValidateAuditAndIssue_When_CredentialsAreValid()
    {
        var command = new GenerateTokenCommand("user@example.com", "password123");
        var userId = _fixture.Create<string>();
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId), new(ClaimTypes.Name, "TestUser") };

        _requestContext.IpAddress.Returns("192.168.1.1");
        _requestContext.UserAgent.Returns("TestAgent");
        _requestContext.ClientId.Returns("test-client");

        _identityService.ValidateCredentialsAsync(command.Email, command.Password, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((userId, claims));
        _tokenIssuance.IssueAndPersistAsync(userId, Arg.Any<IEnumerable<Claim>>(), command.Email, Arg.Any<CancellationToken>())
            .Returns(_fixture.Create<TokenResponse>());

        await _sut.Handle(command, CancellationToken.None);

        await _identityService.Received(1).ValidateCredentialsAsync(command.Email, command.Password, Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _securityAudit.Received(1).LoginSucceededAsync(userId, Arg.Any<string>(), "test-client", "192.168.1.1", "TestAgent", Arg.Any<CancellationToken>());
        await _tokenIssuance.Received(1).IssueAndPersistAsync(userId, Arg.Any<IEnumerable<Claim>>(), command.Email, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorized_When_CredentialsAreInvalid()
    {
        var command = new GenerateTokenCommand("user@example.com", "wrongpassword");

        _requestContext.IpAddress.Returns("192.168.1.1");
        _requestContext.ClientId.Returns("test-client");

        _identityService.ValidateCredentialsAsync(command.Email, command.Password, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((ValueTuple<string, IEnumerable<Claim>>?)null);

        var exception = await Should.ThrowAsync<UnauthorizedAccessException>(
            async () => await _sut.Handle(command, CancellationToken.None));

        exception.Message.ShouldBe("Invalid credentials.");
    }

    [Fact]
    public async Task Handle_Should_AuditFailedLogin_When_CredentialsAreInvalid()
    {
        var command = new GenerateTokenCommand("user@example.com", "wrongpassword");

        _requestContext.IpAddress.Returns("192.168.1.1");
        _requestContext.ClientId.Returns("test-client");

        _identityService.ValidateCredentialsAsync(command.Email, command.Password, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((ValueTuple<string, IEnumerable<Claim>>?)null);

        await Should.ThrowAsync<UnauthorizedAccessException>(
            async () => await _sut.Handle(command, CancellationToken.None));

        await _securityAudit.Received(1).LoginFailedAsync(
            command.Email, "test-client", "InvalidCredentials", "192.168.1.1", Arg.Any<CancellationToken>());
        await _tokenIssuance.DidNotReceive().IssueAndPersistAsync(
            Arg.Any<string>(), Arg.Any<IEnumerable<Claim>>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowArgumentNullException_When_CommandIsNull()
    {
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await _sut.Handle(null!, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Should_DefaultMissingRequestContextValues_ToUnknown()
    {
        var command = new GenerateTokenCommand("user@example.com", "password123");
        var userId = _fixture.Create<string>();
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };

        _requestContext.IpAddress.Returns((string?)null);
        _requestContext.UserAgent.Returns((string?)null);
        _requestContext.ClientId.Returns("test-client");

        _identityService.ValidateCredentialsAsync(command.Email, command.Password, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((userId, claims));
        _tokenIssuance.IssueAndPersistAsync(userId, Arg.Any<IEnumerable<Claim>>(), command.Email, Arg.Any<CancellationToken>())
            .Returns(_fixture.Create<TokenResponse>());

        await _sut.Handle(command, CancellationToken.None);

        await _securityAudit.Received().LoginSucceededAsync(
            userId, Arg.Any<string>(), "test-client", "unknown", "unknown", Arg.Any<CancellationToken>());
    }
}
