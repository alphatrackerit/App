using FSH.Framework.Core.Domain;

namespace FSH.Modules.Cashflow.Domain;

public sealed class Supplier : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public string? Code { get; private set; }

    public string? TaxId { get; private set; }          // NifCif
    public string? Address { get; private set; }        // Direccion
    public string? SupplierType { get; private set; }   // TipoProveedor
    public string? Contact { get; private set; }        // Contacto
    public string? LegalName { get; private set; }      // RazonSocial
    public string? Phone { get; private set; }          // Telefono
    public string? Email { get; private set; }
    public DateTimeOffset? RegisteredOn { get; private set; }   // FechaAlta

    /// <summary>Color painted on the cash-flow calendar for this supplier's payments.</summary>
    public string? ColorHex { get; private set; }

    /// <summary>Higher wins: the day's cell takes the color of the highest-priority supplier paid that day.</summary>
    public int VisualPriority { get; private set; }

    private Supplier() { }

    public static Supplier Create(
        string name, string? code, string? taxId, string? address, string? supplierType,
        string? contact, string? legalName, string? phone, string? email,
        DateTimeOffset? registeredOn, string? colorHex, int visualPriority)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Supplier
        {
            Id = Guid.CreateVersion7(),
            Name = name.Trim(),
            Code = code?.Trim(),
            TaxId = taxId?.Trim(),
            Address = address?.Trim(),
            SupplierType = supplierType?.Trim(),
            Contact = contact?.Trim(),
            LegalName = legalName?.Trim(),
            Phone = phone?.Trim(),
            Email = email?.Trim(),
            RegisteredOn = registeredOn,
            ColorHex = colorHex?.Trim(),
            VisualPriority = visualPriority,
        };
    }

    public void Update(
        string name, string? code, string? taxId, string? address, string? supplierType,
        string? contact, string? legalName, string? phone, string? email,
        DateTimeOffset? registeredOn, string? colorHex, int visualPriority)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Code = code?.Trim();
        TaxId = taxId?.Trim();
        Address = address?.Trim();
        SupplierType = supplierType?.Trim();
        Contact = contact?.Trim();
        LegalName = legalName?.Trim();
        Phone = phone?.Trim();
        Email = email?.Trim();
        RegisteredOn = registeredOn;
        ColorHex = colorHex?.Trim();
        VisualPriority = visualPriority;
    }
}
