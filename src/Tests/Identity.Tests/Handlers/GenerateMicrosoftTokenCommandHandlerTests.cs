using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Multitenancy;
using FSH.Modules.Identity.Authorization.MicrosoftEntra;
using FSH.Modules.Identity.Contracts.v1.Tokens.MicrosoftTokenGeneration;
using FSH.Modules.Identity.Features.v1.Tokens.MicrosoftTokenGeneration;
using FSH.Modules.Multitenancy.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Identity.Tests.Handlers;

/// <summary>
/// Tests the security-critical guards of the Microsoft sign-in handler that run BEFORE the
/// tenant-scoped child scope: token validation, domain → tenant resolution, and the root-tenant
/// boundary. The link-only success path is exercised by the integration tests (it needs a real
/// tenant-scoped Identity store).
/// </summary>
public sealed class GenerateMicrosoftTokenCommandHandlerTests
{
    private readonly IMicrosoftTokenValidator _validator = Substitute.For<IMicrosoftTokenValidator>();
    private readonly ITenantDomainResolver _domainResolver = Substitute.For<ITenantDomainResolver>();
    private readonly IMultiTenantStore<AppTenantInfo> _tenantStore = Substitute.For<IMultiTenantStore<AppTenantInfo>>();
    private readonly GenerateMicrosoftTokenCommandHandler _sut;

    public GenerateMicrosoftTokenCommandHandlerTests()
    {
        _sut = new GenerateMicrosoftTokenCommandHandler(
            _validator,
            _domainResolver,
            _tenantStore,
            Substitute.For<IServiceScopeFactory>(),
            Substitute.For<ILogger<GenerateMicrosoftTokenCommandHandler>>());
    }

    [Fact]
    public async Task Handle_Should_Propagate_When_TokenInvalid()
    {
        _validator.ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new UnauthorizedException("Microsoft sign-in failed."));

        await Should.ThrowAsync<UnauthorizedException>(
            async () => await _sut.Handle(new GenerateMicrosoftTokenCommand("bad-token"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Should_Reject_When_NoTenantMappedForDomain()
    {
        _validator.ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new MicrosoftPrincipal("user@unknown.com", "oid-1", "Test User"));
        _domainResolver.ResolveTenantIdByDomainAsync("unknown.com", Arg.Any<CancellationToken>())
            .Returns((string?)null);

        await Should.ThrowAsync<UnauthorizedException>(
            async () => await _sut.Handle(new GenerateMicrosoftTokenCommand("id-token"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Should_Reject_When_DomainMapsToRootTenant()
    {
        _validator.ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new MicrosoftPrincipal("admin@root.example", "oid-2", "Root Admin"));
        _domainResolver.ResolveTenantIdByDomainAsync("root.example", Arg.Any<CancellationToken>())
            .Returns(MultitenancyConstants.Root.Id);

        await Should.ThrowAsync<UnauthorizedException>(
            async () => await _sut.Handle(new GenerateMicrosoftTokenCommand("id-token"), CancellationToken.None));
    }
}
