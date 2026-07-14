using FSH.Framework.Core.Domain;
using FSH.Modules.Avicola.Contracts;

namespace FSH.Modules.Avicola.Domain;

/// <summary>A feed-consumption record for a flock (consumo de alimento).</summary>
public sealed class RegistroAlimentacion : AggregateRoot<Guid>
{
    /// <summary>Owning flock (same module → real FK, ON DELETE NO ACTION).</summary>
    public Guid LoteId { get; private set; }

    public DateTimeOffset Fecha { get; private set; }
    public TipoAlimento TipoAlimento { get; private set; }

    /// <summary>Feed consumed, in kilograms.</summary>
    public decimal CantidadKg { get; private set; }

    /// <summary>Cost per kilogram of feed.</summary>
    public decimal? CostoUnitario { get; private set; }

    public string? Notas { get; private set; }

    private RegistroAlimentacion() { }

    public static RegistroAlimentacion Create(
        Guid loteId,
        DateTimeOffset fecha,
        TipoAlimento tipoAlimento,
        decimal cantidadKg,
        decimal? costoUnitario,
        string? notas)
    {
        return new RegistroAlimentacion
        {
            Id = Guid.CreateVersion7(),
            LoteId = loteId,
            Fecha = fecha,
            TipoAlimento = tipoAlimento,
            CantidadKg = cantidadKg,
            CostoUnitario = costoUnitario,
            Notas = notas?.Trim(),
        };
    }

    public void Update(
        Guid loteId,
        DateTimeOffset fecha,
        TipoAlimento tipoAlimento,
        decimal cantidadKg,
        decimal? costoUnitario,
        string? notas)
    {
        LoteId = loteId;
        Fecha = fecha;
        TipoAlimento = tipoAlimento;
        CantidadKg = cantidadKg;
        CostoUnitario = costoUnitario;
        Notas = notas?.Trim();
    }
}
