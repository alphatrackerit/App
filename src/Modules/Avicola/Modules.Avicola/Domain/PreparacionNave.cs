using FSH.Framework.Core.Domain;
using FSH.Modules.Avicola.Contracts;

namespace FSH.Modules.Avicola.Domain;

/// <summary>
/// A between-flocks shed preparation (vacío sanitario): the cleaning + disinfection + prep cycle that
/// runs after a flock is withdrawn and before the next one is placed. Tracks the downtime dates, the
/// cleaning checklist, the cost and the lifecycle state. The cleaning certificate is attached as a
/// <see cref="Documento"/> with origin <c>Preparacion</c>.
/// </summary>
public sealed class PreparacionNave : AggregateRoot<Guid>
{
    /// <summary>Shed being prepared (same module → real FK, NO ACTION).</summary>
    public Guid GalponId { get; private set; }

    /// <summary>Flock that just left, if any (same module → real FK, NO ACTION).</summary>
    public Guid? LoteAnteriorId { get; private set; }

    /// <summary>When the birds were withdrawn by the external company (end of production).</summary>
    public DateTimeOffset? FechaRetiro { get; private set; }

    /// <summary>When cleaning/preparation started.</summary>
    public DateTimeOffset FechaInicio { get; private set; }

    /// <summary>When the shed became ready for the next flock.</summary>
    public DateTimeOffset? FechaFin { get; private set; }

    // Cleaning checklist (vacío sanitario steps).
    public bool RetiradaCama { get; private set; }
    public bool Lavado { get; private set; }
    public bool Desinfeccion { get; private set; }
    public bool Desinsectacion { get; private set; }
    public bool CamaNueva { get; private set; }

    public decimal? Costo { get; private set; }
    public EstadoPreparacion Estado { get; private set; }
    public string? Notas { get; private set; }

    private PreparacionNave() { }

    public static PreparacionNave Create(
        Guid galponId,
        Guid? loteAnteriorId,
        DateTimeOffset? fechaRetiro,
        DateTimeOffset fechaInicio,
        bool retiradaCama,
        bool lavado,
        bool desinfeccion,
        bool desinsectacion,
        bool camaNueva,
        decimal? costo,
        EstadoPreparacion estado,
        string? notas)
    {
        return new PreparacionNave
        {
            Id = Guid.CreateVersion7(),
            GalponId = galponId,
            LoteAnteriorId = loteAnteriorId,
            FechaRetiro = fechaRetiro,
            FechaInicio = fechaInicio,
            FechaFin = estado == EstadoPreparacion.Completada ? fechaInicio : null,
            RetiradaCama = retiradaCama,
            Lavado = lavado,
            Desinfeccion = desinfeccion,
            Desinsectacion = desinsectacion,
            CamaNueva = camaNueva,
            Costo = costo,
            Estado = estado,
            Notas = notas?.Trim(),
        };
    }

    public void Update(
        Guid galponId,
        Guid? loteAnteriorId,
        DateTimeOffset? fechaRetiro,
        DateTimeOffset fechaInicio,
        bool retiradaCama,
        bool lavado,
        bool desinfeccion,
        bool desinsectacion,
        bool camaNueva,
        decimal? costo,
        string? notas)
    {
        GalponId = galponId;
        LoteAnteriorId = loteAnteriorId;
        FechaRetiro = fechaRetiro;
        FechaInicio = fechaInicio;
        RetiradaCama = retiradaCama;
        Lavado = lavado;
        Desinfeccion = desinfeccion;
        Desinsectacion = desinsectacion;
        CamaNueva = camaNueva;
        Costo = costo;
        Notas = notas?.Trim();
    }

    /// <summary>Marks the shed ready for the next flock.</summary>
    public void Completar(DateTimeOffset fechaFin)
    {
        Estado = EstadoPreparacion.Completada;
        FechaFin = fechaFin;
    }
}
