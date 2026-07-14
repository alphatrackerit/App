using FSH.Framework.Core.Domain;
using FSH.Modules.Avicola.Contracts;

namespace FSH.Modules.Avicola.Domain;

/// <summary>
/// A broiler flock / batch (lote de engorde) — the central aggregate of the module. A flock is
/// placed in a <see cref="Galpon"/> as day-old chicks and reared until harvested/dispatched.
/// </summary>
public sealed class Lote : AggregateRoot<Guid>
{
    public string Codigo { get; private set; } = default!;

    /// <summary>Owning shed (same module → real FK, ON DELETE NO ACTION). Nullable.</summary>
    public Guid? GalponId { get; private set; }

    /// <summary>Breed / genetic line (e.g. Ross 308, Cobb 500).</summary>
    public string? Raza { get; private set; }

    /// <summary>Placement date — when day-old chicks arrived.</summary>
    public DateTimeOffset FechaIngreso { get; private set; }

    /// <summary>Number of chicks placed.</summary>
    public int CantidadInicial { get; private set; }

    /// <summary>Average placement weight in grams.</summary>
    public decimal? PesoInicialGramos { get; private set; }

    public DateTimeOffset? FechaSalidaPrevista { get; private set; }
    public DateTimeOffset? FechaSalidaReal { get; private set; }

    public EstadoLote Estado { get; private set; }

    /// <summary>Soft reference to an external supplier/hatchery catalog (indexed, no FK).</summary>
    public Guid? ProveedorId { get; private set; }

    /// <summary>Per-chick cost at placement.</summary>
    public decimal? CostoPolluelo { get; private set; }

    public string? Notas { get; private set; }

    private Lote() { }

    public static Lote Create(
        string codigo,
        Guid? galponId,
        string? raza,
        DateTimeOffset fechaIngreso,
        int cantidadInicial,
        decimal? pesoInicialGramos,
        DateTimeOffset? fechaSalidaPrevista,
        EstadoLote estado,
        Guid? proveedorId,
        decimal? costoPolluelo,
        string? notas)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);

        return new Lote
        {
            Id = Guid.CreateVersion7(),
            Codigo = codigo.Trim(),
            GalponId = galponId,
            Raza = raza?.Trim(),
            FechaIngreso = fechaIngreso,
            CantidadInicial = cantidadInicial,
            PesoInicialGramos = pesoInicialGramos,
            FechaSalidaPrevista = fechaSalidaPrevista,
            FechaSalidaReal = null,
            Estado = estado,
            ProveedorId = proveedorId,
            CostoPolluelo = costoPolluelo,
            Notas = notas?.Trim(),
        };
    }

    public void Update(
        string codigo,
        Guid? galponId,
        string? raza,
        DateTimeOffset fechaIngreso,
        int cantidadInicial,
        decimal? pesoInicialGramos,
        DateTimeOffset? fechaSalidaPrevista,
        EstadoLote estado,
        Guid? proveedorId,
        decimal? costoPolluelo,
        string? notas)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);

        Codigo = codigo.Trim();
        GalponId = galponId;
        Raza = raza?.Trim();
        FechaIngreso = fechaIngreso;
        CantidadInicial = cantidadInicial;
        PesoInicialGramos = pesoInicialGramos;
        FechaSalidaPrevista = fechaSalidaPrevista;
        Estado = estado;
        ProveedorId = proveedorId;
        CostoPolluelo = costoPolluelo;
        Notas = notas?.Trim();
    }

    /// <summary>Closes the flock: records the actual exit date and marks it finished.</summary>
    public void Cerrar(DateTimeOffset fechaSalidaReal)
    {
        FechaSalidaReal = fechaSalidaReal;
        Estado = EstadoLote.Finalizado;
    }
}
