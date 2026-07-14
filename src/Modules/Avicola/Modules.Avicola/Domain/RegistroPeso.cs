using FSH.Framework.Core.Domain;

namespace FSH.Modules.Avicola.Domain;

/// <summary>A weight-sampling record for a flock (muestreo de peso).</summary>
public sealed class RegistroPeso : AggregateRoot<Guid>
{
    /// <summary>Owning flock (same module → real FK, ON DELETE NO ACTION).</summary>
    public Guid LoteId { get; private set; }

    public DateTimeOffset Fecha { get; private set; }

    /// <summary>Average bird weight in grams at this sampling.</summary>
    public decimal PesoPromedioGramos { get; private set; }

    /// <summary>Number of birds weighed in the sample.</summary>
    public int? CantidadMuestra { get; private set; }

    public string? Notas { get; private set; }

    private RegistroPeso() { }

    public static RegistroPeso Create(
        Guid loteId,
        DateTimeOffset fecha,
        decimal pesoPromedioGramos,
        int? cantidadMuestra,
        string? notas)
    {
        return new RegistroPeso
        {
            Id = Guid.CreateVersion7(),
            LoteId = loteId,
            Fecha = fecha,
            PesoPromedioGramos = pesoPromedioGramos,
            CantidadMuestra = cantidadMuestra,
            Notas = notas?.Trim(),
        };
    }

    public void Update(
        Guid loteId,
        DateTimeOffset fecha,
        decimal pesoPromedioGramos,
        int? cantidadMuestra,
        string? notas)
    {
        LoteId = loteId;
        Fecha = fecha;
        PesoPromedioGramos = pesoPromedioGramos;
        CantidadMuestra = cantidadMuestra;
        Notas = notas?.Trim();
    }
}
