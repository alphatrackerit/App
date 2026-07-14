using FSH.Framework.Core.Domain;

namespace FSH.Modules.Avicola.Domain;

/// <summary>A daily mortality / cull record for a flock.</summary>
public sealed class RegistroMortalidad : AggregateRoot<Guid>
{
    /// <summary>Owning flock (same module → real FK, ON DELETE NO ACTION).</summary>
    public Guid LoteId { get; private set; }

    public DateTimeOffset Fecha { get; private set; }

    /// <summary>Number of dead birds.</summary>
    public int Cantidad { get; private set; }

    /// <summary>Number of culled birds (descartes).</summary>
    public int? Descartes { get; private set; }

    public string? Causa { get; private set; }
    public string? Notas { get; private set; }

    private RegistroMortalidad() { }

    public static RegistroMortalidad Create(
        Guid loteId,
        DateTimeOffset fecha,
        int cantidad,
        int? descartes,
        string? causa,
        string? notas)
    {
        return new RegistroMortalidad
        {
            Id = Guid.CreateVersion7(),
            LoteId = loteId,
            Fecha = fecha,
            Cantidad = cantidad,
            Descartes = descartes,
            Causa = causa?.Trim(),
            Notas = notas?.Trim(),
        };
    }

    public void Update(
        Guid loteId,
        DateTimeOffset fecha,
        int cantidad,
        int? descartes,
        string? causa,
        string? notas)
    {
        LoteId = loteId;
        Fecha = fecha;
        Cantidad = cantidad;
        Descartes = descartes;
        Causa = causa?.Trim();
        Notas = notas?.Trim();
    }
}
