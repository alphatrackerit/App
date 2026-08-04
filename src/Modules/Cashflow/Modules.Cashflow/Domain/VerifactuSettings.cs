using FSH.Framework.Core.Domain;
using FSH.Modules.Cashflow.Contracts.Enums;

namespace FSH.Modules.Cashflow.Domain;

/// <summary>
/// Per-<see cref="Company"/> VERI*FACTU configuration and chain state (1:1, real FK). VeriFactu
/// obliges per NIF, so the hash chain, certificate and on/off switch live here — NOT per tenant
/// (a tenant may hold several legal entities). Tenant isolation still applies automatically via
/// <c>BaseDbContext</c>. The certificate and its password are DataProtection-encrypted at rest
/// (<c>VerifactuSecretProtector</c>) — never stored in the clear, never returned by the API.
/// </summary>
public sealed class VerifactuSettings : AggregateRoot<Guid>
{
    public Guid CompanyId { get; private set; }
    public VerifactuEnvironment Environment { get; private set; } = VerifactuEnvironment.Pruebas;

    /// <summary>Off by default: records are still chained locally (<c>PendienteEnvio</c>) and the
    /// sweep job submits them once this turns on — no code change needed when the certificate arrives.</summary>
    public bool Enabled { get; private set; }

    // Identification of the SIF (sistema informático de facturación) the AEAT requires per record.
    public string? SoftwareName { get; private set; }
    public string? SoftwareVersion { get; private set; }
    public string? InstallationNumber { get; private set; }

    /// <summary>PFX bytes, DataProtection-encrypted.</summary>
    public byte[]? EncryptedCertificate { get; private set; }

    /// <summary>PFX password, DataProtection-encrypted.</summary>
    public string? EncryptedCertificatePassword { get; private set; }

    /// <summary>Hash of the LAST chained record for this company — the next record's PreviousHash.
    /// Null until the first record is issued.</summary>
    public string? LastChainHash { get; private set; }

    public DateTimeOffset? LastChainAt { get; private set; }

    /// <summary>Postgres xmin concurrency token — belt-and-braces on top of the per-company
    /// advisory lock that serialises chaining.</summary>
#pragma warning disable S1144 // EF Core writes the xmin token through the setter at materialisation
    public uint RowVersion { get; private set; }
#pragma warning restore S1144

    private VerifactuSettings() { }

    public static VerifactuSettings Create(Guid companyId)
    {
        return new VerifactuSettings
        {
            Id = Guid.CreateVersion7(),
            CompanyId = companyId,
        };
    }

    public void Update(VerifactuEnvironment environment, bool enabled, string? softwareName, string? softwareVersion, string? installationNumber)
    {
        Environment = environment;
        Enabled = enabled;
        SoftwareName = softwareName?.Trim();
        SoftwareVersion = softwareVersion?.Trim();
        InstallationNumber = installationNumber?.Trim();
    }

    public void SetCertificate(byte[] encryptedPfx, string encryptedPassword)
    {
        ArgumentNullException.ThrowIfNull(encryptedPfx);
        ArgumentException.ThrowIfNullOrWhiteSpace(encryptedPassword);
        EncryptedCertificate = encryptedPfx;
        EncryptedCertificatePassword = encryptedPassword;
    }

    public bool HasCertificate => EncryptedCertificate is { Length: > 0 };

    /// <summary>Advances the chain head after persisting a new record. Caller MUST hold the
    /// per-company advisory lock and run in the same transaction as the record insert.</summary>
    public void AdvanceChain(string hash, DateTimeOffset at)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hash);
        LastChainHash = hash;
        LastChainAt = at;
    }
}
