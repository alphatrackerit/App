using FSH.Framework.Core.Domain;

namespace FSH.Modules.Avicola.Domain;

/// <summary>A harvest / dispatch record — birds leaving the flock to processing (faena / despacho).</summary>
public sealed class Despacho : AggregateRoot<Guid>
{
    /// <summary>Owning flock (same module → real FK, ON DELETE NO ACTION).</summary>
    public Guid LoteId { get; private set; }

    public DateTimeOffset Fecha { get; private set; }

    /// <summary>Number of birds dispatched.</summary>
    public int Cantidad { get; private set; }

    /// <summary>Total live weight dispatched, in kilograms.</summary>
    public decimal PesoTotalKg { get; private set; }

    /// <summary>Price per kilogram sold.</summary>
    public decimal? PrecioPorKg { get; private set; }

    /// <summary>Soft reference to an external client/buyer catalog (indexed, no FK).</summary>
    public Guid? ClienteId { get; private set; }

    public string? Notas { get; private set; }

    private Despacho() { }

    public static Despacho Create(
        Guid loteId,
        DateTimeOffset fecha,
        int cantidad,
        decimal pesoTotalKg,
        decimal? precioPorKg,
        Guid? clienteId,
        string? notas)
    {
        return new Despacho
        {
            Id = Guid.CreateVersion7(),
            LoteId = loteId,
            Fecha = fecha,
            Cantidad = cantidad,
            PesoTotalKg = pesoTotalKg,
            PrecioPorKg = precioPorKg,
            ClienteId = clienteId,
            Notas = notas?.Trim(),
        };
    }

    public void Update(
        Guid loteId,
        DateTimeOffset fecha,
        int cantidad,
        decimal pesoTotalKg,
        decimal? precioPorKg,
        Guid? clienteId,
        string? notas)
    {
        LoteId = loteId;
        Fecha = fecha;
        Cantidad = cantidad;
        PesoTotalKg = pesoTotalKg;
        PrecioPorKg = precioPorKg;
        ClienteId = clienteId;
        Notas = notas?.Trim();
    }
}
