using FSH.Framework.Core.Domain;
using FSH.Modules.Avicola.Contracts;

namespace FSH.Modules.Avicola.Domain;

/// <summary>
/// A manual accounting movement (libro de movimientos) — an income or expense the admin records
/// on top of the auto-computed costs/revenue. Optionally tied to a flock for per-lote P&amp;L.
/// </summary>
public sealed class MovimientoContable : AggregateRoot<Guid>
{
    public DateTimeOffset Fecha { get; private set; }
    public TipoMovimiento Tipo { get; private set; }
    public CategoriaMovimiento Categoria { get; private set; }
    public string Concepto { get; private set; } = default!;
    public decimal Importe { get; private set; }

    /// <summary>Optional flock this movement belongs to (same module → real FK, NO ACTION).</summary>
    public Guid? LoteId { get; private set; }

    public string? Notas { get; private set; }

    private MovimientoContable() { }

    public static MovimientoContable Create(
        DateTimeOffset fecha,
        TipoMovimiento tipo,
        CategoriaMovimiento categoria,
        string concepto,
        decimal importe,
        Guid? loteId,
        string? notas)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(concepto);

        return new MovimientoContable
        {
            Id = Guid.CreateVersion7(),
            Fecha = fecha,
            Tipo = tipo,
            Categoria = categoria,
            Concepto = concepto.Trim(),
            Importe = importe,
            LoteId = loteId,
            Notas = notas?.Trim(),
        };
    }

    public void Update(
        DateTimeOffset fecha,
        TipoMovimiento tipo,
        CategoriaMovimiento categoria,
        string concepto,
        decimal importe,
        Guid? loteId,
        string? notas)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(concepto);

        Fecha = fecha;
        Tipo = tipo;
        Categoria = categoria;
        Concepto = concepto.Trim();
        Importe = importe;
        LoteId = loteId;
        Notas = notas?.Trim();
    }
}
