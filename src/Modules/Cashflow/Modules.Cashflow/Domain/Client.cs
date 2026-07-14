using FSH.Framework.Core.Domain;

namespace FSH.Modules.Cashflow.Domain;

public sealed class Client : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public string? Code { get; private set; }

    public string? TaxId { get; private set; }          // NifCif
    public string? Address { get; private set; }        // Direccion
    public string? ClientType { get; private set; }     // TipoCliente
    public string? Contact { get; private set; }        // Contacto
    public string? LegalName { get; private set; }      // RazonSocial
    public string? Phone { get; private set; }          // Telefono
    public string? Email { get; private set; }
    public DateTimeOffset? RegisteredOn { get; private set; }   // FechaAlta
    public string? ColorHex { get; private set; }

    private Client() { }

    public static Client Create(
        string name, string? code, string? taxId, string? address, string? clientType,
        string? contact, string? legalName, string? phone, string? email,
        DateTimeOffset? registeredOn, string? colorHex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Client
        {
            Id = Guid.CreateVersion7(),
            Name = name.Trim(),
            Code = code?.Trim(),
            TaxId = taxId?.Trim(),
            Address = address?.Trim(),
            ClientType = clientType?.Trim(),
            Contact = contact?.Trim(),
            LegalName = legalName?.Trim(),
            Phone = phone?.Trim(),
            Email = email?.Trim(),
            RegisteredOn = registeredOn,
            ColorHex = colorHex?.Trim(),
        };
    }

    public void Update(
        string name, string? code, string? taxId, string? address, string? clientType,
        string? contact, string? legalName, string? phone, string? email,
        DateTimeOffset? registeredOn, string? colorHex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Code = code?.Trim();
        TaxId = taxId?.Trim();
        Address = address?.Trim();
        ClientType = clientType?.Trim();
        Contact = contact?.Trim();
        LegalName = legalName?.Trim();
        Phone = phone?.Trim();
        Email = email?.Trim();
        RegisteredOn = registeredOn;
        ColorHex = colorHex?.Trim();
    }
}
