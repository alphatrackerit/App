using FSH.Framework.Core.Domain;

namespace FSH.Modules.Cashflow.Domain;

public sealed class Company : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public string? Code { get; private set; }

    public string? LegalName { get; private set; }          // RazonSocial
    public string? TaxRegistration { get; private set; }    // RegistroFiscal
    public bool ShowInProjects { get; private set; }        // VisibleEnProyectos — appears as a card in Proyectos

    /// <summary>Tax id (NIF) used for AEAT VERI*FACTU records. Deliberately separate from the
    /// free-semantics <see cref="TaxRegistration"/> ("RegistroFiscal") — a legal registry field
    /// must not reuse a column with ambiguous meaning.</summary>
    public string? Nif { get; private set; }

    // ── Datos de emisor para el PDF de factura (cabecera presentable) ──
    public string? Address { get; private set; }        // Direccion
    public string? PostalCode { get; private set; }     // CodigoPostal
    public string? City { get; private set; }           // Ciudad
    public string? Phone { get; private set; }          // Telefono
    public string? Email { get; private set; }

    /// <summary>Storage key of the company logo embedded in invoice/proforma PDFs.</summary>
    public string? LogoPath { get; private set; }

    private Company() { }

    public static Company Create(
        string name, string? code, string? legalName, string? taxRegistration, bool showInProjects = true,
        string? nif = null, string? address = null, string? postalCode = null, string? city = null,
        string? phone = null, string? email = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Company
        {
            Id = Guid.CreateVersion7(),
            Name = name.Trim(),
            Code = code?.Trim(),
            LegalName = legalName?.Trim(),
            TaxRegistration = taxRegistration?.Trim(),
            ShowInProjects = showInProjects,
            Nif = NormalizeNif(nif),
            Address = address?.Trim(),
            PostalCode = postalCode?.Trim(),
            City = city?.Trim(),
            Phone = phone?.Trim(),
            Email = email?.Trim(),
        };
    }

    public void Update(
        string name, string? code, string? legalName, string? taxRegistration, bool showInProjects,
        string? nif, string? address, string? postalCode, string? city, string? phone, string? email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Code = code?.Trim();
        LegalName = legalName?.Trim();
        TaxRegistration = taxRegistration?.Trim();
        ShowInProjects = showInProjects;
        Nif = NormalizeNif(nif);
        Address = address?.Trim();
        PostalCode = postalCode?.Trim();
        City = city?.Trim();
        Phone = phone?.Trim();
        Email = email?.Trim();
    }

    /// <summary>Sets (or clears) the logo. Kept off <see cref="Update"/> so a routine edit never
    /// silently detaches the file — same rule as Invoice.AttachDocument.</summary>
    public void SetLogo(string? logoPath) => LogoPath = logoPath;

    // The AEAT matches NIFs literally: uppercase, no spaces or separators.
    private static string? NormalizeNif(string? nif)
    {
        string? trimmed = nif?.Trim().Replace(" ", "", StringComparison.Ordinal).Replace("-", "", StringComparison.Ordinal).ToUpperInvariant();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
