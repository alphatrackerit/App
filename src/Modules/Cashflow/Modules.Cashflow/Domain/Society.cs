using FSH.Framework.Core.Domain;

namespace FSH.Modules.Cashflow.Domain;

/// <summary>A legal entity ("Sociedad") belonging to a <see cref="Client"/>.</summary>
public sealed class Society : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public string? TaxId { get; private set; }          // NifCif
    public string? Address { get; private set; }        // Direccion
    public string? PostalCode { get; private set; }     // CodigoPostal
    public string? City { get; private set; }           // Ciudad
    public string? Country { get; private set; }        // Pais (free text per spec)

    /// <summary>Owning client (same module → real FK, ON DELETE NO ACTION).</summary>
    public Guid? ClientId { get; private set; }

    private Society() { }

    public static Society Create(
        string name, string? taxId, string? address, string? postalCode, string? city, string? country, Guid? clientId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Society
        {
            Id = Guid.CreateVersion7(),
            Name = name.Trim(),
            TaxId = taxId?.Trim(),
            Address = address?.Trim(),
            PostalCode = postalCode?.Trim(),
            City = city?.Trim(),
            Country = country?.Trim(),
            ClientId = clientId,
        };
    }

    public void Update(
        string name, string? taxId, string? address, string? postalCode, string? city, string? country, Guid? clientId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        TaxId = taxId?.Trim();
        Address = address?.Trim();
        PostalCode = postalCode?.Trim();
        City = city?.Trim();
        Country = country?.Trim();
        ClientId = clientId;
    }
}
