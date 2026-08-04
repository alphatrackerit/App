using FSH.Modules.Cashflow.Contracts.Enums;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Verifactu;

/// <summary>Per-company VeriFactu settings. The certificate itself is never returned — only
/// whether one is stored (<see cref="HasCertificate"/>).</summary>
public sealed record VerifactuSettingsDto(
    Guid CompanyId,
    VerifactuEnvironment Environment,
    bool Enabled,
    string? SoftwareName,
    string? SoftwareVersion,
    string? InstallationNumber,
    bool HasCertificate,
    string? LastChainHash,
    DateTimeOffset? LastChainAtUtc);

public sealed record GetVerifactuSettingsQuery(Guid CompanyId) : IQuery<VerifactuSettingsDto>;

/// <summary>Creates or updates a company's VeriFactu settings (certificate travels separately via
/// <see cref="SetVerifactuCertificateCommand"/>). Enabling requires a stored certificate.</summary>
public sealed record UpsertVerifactuSettingsCommand(
    Guid CompanyId,
    VerifactuEnvironment Environment,
    bool Enabled,
    string? SoftwareName,
    string? SoftwareVersion,
    string? InstallationNumber) : ICommand<Guid>;

/// <summary>Stores the company's client certificate (PFX + password), encrypted at rest via
/// DataProtection. Multipart upload — same pattern as invoice document attachment.</summary>
public sealed record SetVerifactuCertificateCommand(
    Guid CompanyId,
    byte[] PfxContent,
    string Password) : ICommand<Unit>;

/// <summary>
/// Issues the invoice under VERI*FACTU: builds the chained billing record (SHA-256 over the
/// previous record's hash), the QR payload, and leaves it <c>PendienteEnvio</c> for the submission
/// job. IRREVERSIBLE: once issued the invoice can no longer be edited or deleted — fixing a
/// mistake requires a rectificativa (out of scope for now).
/// </summary>
public sealed record IssueInvoiceVerifactuCommand(Guid InvoiceId) : ICommand<InvoiceVerifactuRecordDto>;

public sealed record InvoiceVerifactuRecordDto(
    Guid Id,
    Guid InvoiceId,
    string? PreviousHash,
    string Hash,
    DateTimeOffset GeneratedAt,
    string QrPayload,
    VerifactuStatus Status,
    string? AeatResponseCode,
    int RetryCount);

