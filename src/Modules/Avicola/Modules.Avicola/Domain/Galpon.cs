using FSH.Framework.Core.Domain;

namespace FSH.Modules.Avicola.Domain;

/// <summary>A poultry house / shed (galpón) where flocks are reared.</summary>
public sealed class Galpon : AggregateRoot<Guid>
{
    public string Nombre { get; private set; } = default!;
    public string? Codigo { get; private set; }

    /// <summary>Maximum bird capacity of the shed.</summary>
    public int Capacidad { get; private set; }

    /// <summary>Floor area in square metres.</summary>
    public decimal? SuperficieM2 { get; private set; }

    public string? Ubicacion { get; private set; }
    public bool Activo { get; private set; }
    public string? Notas { get; private set; }

    private Galpon() { }

    public static Galpon Create(
        string nombre,
        string? codigo,
        int capacidad,
        decimal? superficieM2,
        string? ubicacion,
        bool activo,
        string? notas)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);

        return new Galpon
        {
            Id = Guid.CreateVersion7(),
            Nombre = nombre.Trim(),
            Codigo = codigo?.Trim(),
            Capacidad = capacidad,
            SuperficieM2 = superficieM2,
            Ubicacion = ubicacion?.Trim(),
            Activo = activo,
            Notas = notas?.Trim(),
        };
    }

    public void Update(
        string nombre,
        string? codigo,
        int capacidad,
        decimal? superficieM2,
        string? ubicacion,
        bool activo,
        string? notas)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);

        Nombre = nombre.Trim();
        Codigo = codigo?.Trim();
        Capacidad = capacidad;
        SuperficieM2 = superficieM2;
        Ubicacion = ubicacion?.Trim();
        Activo = activo;
        Notas = notas?.Trim();
    }
}
