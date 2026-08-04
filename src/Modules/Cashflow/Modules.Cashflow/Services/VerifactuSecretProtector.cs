using Microsoft.AspNetCore.DataProtection;

namespace FSH.Modules.Cashflow.Services;

/// <summary>
/// Encrypts/decrypts the per-company VeriFactu client certificate (PFX) and its password at rest.
/// The certificate must be recoverable (it authenticates the mutual-TLS connection to the AEAT
/// webservice), so we protect it with ASP.NET Data Protection (key ring persisted to Redis in
/// production) — same pattern as <c>WebhookSecretProtector</c>.
/// </summary>
public interface IVerifactuSecretProtector
{
    byte[] Protect(byte[] plaintext);
    byte[] Unprotect(byte[] ciphertext);
    string ProtectString(string plaintext);
    string UnprotectString(string ciphertext);
}

public sealed class VerifactuSecretProtector : IVerifactuSecretProtector
{
    private readonly IDataProtector _protector;

    public VerifactuSecretProtector(IDataProtectionProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _protector = provider.CreateProtector("FSH.Cashflow.VerifactuCertificate.v1");
    }

    public byte[] Protect(byte[] plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext);
        return _protector.Protect(plaintext);
    }

    public byte[] Unprotect(byte[] ciphertext)
    {
        ArgumentNullException.ThrowIfNull(ciphertext);
        return _protector.Unprotect(ciphertext);
    }

    public string ProtectString(string plaintext)
    {
        ArgumentException.ThrowIfNullOrEmpty(plaintext);
        return _protector.Protect(plaintext);
    }

    public string UnprotectString(string ciphertext)
    {
        ArgumentException.ThrowIfNullOrEmpty(ciphertext);
        return _protector.Unprotect(ciphertext);
    }
}
