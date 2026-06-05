using FSH.Modules.Identity.Contracts.DTOs;
using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Tokens.MicrosoftTokenGeneration;

/// <summary>
/// Exchanges a Microsoft (Entra ID) ID token — obtained by the dashboard via MSAL — for an FSH
/// access/refresh token pair. The tenant is derived from the verified email domain; only existing,
/// active users are accepted (no auto-provisioning).
/// </summary>
public sealed record GenerateMicrosoftTokenCommand(string IdToken) : ICommand<TokenResponse>;
