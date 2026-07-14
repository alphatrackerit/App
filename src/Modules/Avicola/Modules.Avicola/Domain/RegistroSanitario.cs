using FSH.Framework.Core.Domain;
using FSH.Modules.Avicola.Contracts;

namespace FSH.Modules.Avicola.Domain;

/// <summary>A health record for a flock: vaccination, medication or treatment (plan sanitario).</summary>
public sealed class RegistroSanitario : AggregateRoot<Guid>
{
    /// <summary>Owning flock (same module → real FK, ON DELETE NO ACTION).</summary>
    public Guid LoteId { get; private set; }

    public DateTimeOffset Fecha { get; private set; }
    public TipoRegistroSanitario Tipo { get; private set; }

    /// <summary>Product applied (vaccine / medicine name).</summary>
    public string Producto { get; private set; } = default!;

    public string? Dosis { get; private set; }

    /// <summary>Route of application (e.g. agua, aspersión, inyección).</summary>
    public string? ViaAplicacion { get; private set; }

    public decimal? Costo { get; private set; }
    public string? Notas { get; private set; }

    private RegistroSanitario() { }

    public static RegistroSanitario Create(
        Guid loteId,
        DateTimeOffset fecha,
        TipoRegistroSanitario tipo,
        string producto,
        string? dosis,
        string? viaAplicacion,
        decimal? costo,
        string? notas)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(producto);

        return new RegistroSanitario
        {
            Id = Guid.CreateVersion7(),
            LoteId = loteId,
            Fecha = fecha,
            Tipo = tipo,
            Producto = producto.Trim(),
            Dosis = dosis?.Trim(),
            ViaAplicacion = viaAplicacion?.Trim(),
            Costo = costo,
            Notas = notas?.Trim(),
        };
    }

    public void Update(
        Guid loteId,
        DateTimeOffset fecha,
        TipoRegistroSanitario tipo,
        string producto,
        string? dosis,
        string? viaAplicacion,
        decimal? costo,
        string? notas)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(producto);

        LoteId = loteId;
        Fecha = fecha;
        Tipo = tipo;
        Producto = producto.Trim();
        Dosis = dosis?.Trim();
        ViaAplicacion = viaAplicacion?.Trim();
        Costo = costo;
        Notas = notas?.Trim();
    }
}
