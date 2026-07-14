using FSH.Framework.Core.Domain;
using FSH.Modules.Avicola.Contracts;

namespace FSH.Modules.Avicola.Domain;

/// <summary>A purchase order (pedido) — feed, chicks, medication or supplies, with its lifecycle.</summary>
public sealed class Pedido : AggregateRoot<Guid>
{
    public string Codigo { get; private set; } = default!;
    public TipoPedido Tipo { get; private set; }

    /// <summary>Soft reference to the external supplier catalog (indexed, no FK).</summary>
    public Guid? ProveedorId { get; private set; }

    public string? Descripcion { get; private set; }
    public decimal Cantidad { get; private set; }

    /// <summary>Unit of measure (kg, sacos, unidades, dosis…).</summary>
    public string? Unidad { get; private set; }

    public decimal? CostoEstimado { get; private set; }
    public decimal? CostoReal { get; private set; }

    public EstadoPedido Estado { get; private set; }
    public DateTimeOffset FechaPedido { get; private set; }
    public DateTimeOffset? FechaRecepcion { get; private set; }

    /// <summary>Optional flock this order is for (same module → real FK, NO ACTION).</summary>
    public Guid? LoteId { get; private set; }

    /// <summary>Optional shed this order is for (same module → real FK, NO ACTION).</summary>
    public Guid? GalponId { get; private set; }

    public string? Notas { get; private set; }

    private Pedido() { }

    public static Pedido Create(
        string codigo,
        TipoPedido tipo,
        Guid? proveedorId,
        string? descripcion,
        decimal cantidad,
        string? unidad,
        decimal? costoEstimado,
        EstadoPedido estado,
        DateTimeOffset fechaPedido,
        Guid? loteId,
        Guid? galponId,
        string? notas)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);

        return new Pedido
        {
            Id = Guid.CreateVersion7(),
            Codigo = codigo.Trim(),
            Tipo = tipo,
            ProveedorId = proveedorId,
            Descripcion = descripcion?.Trim(),
            Cantidad = cantidad,
            Unidad = unidad?.Trim(),
            CostoEstimado = costoEstimado,
            CostoReal = null,
            Estado = estado,
            FechaPedido = fechaPedido,
            FechaRecepcion = null,
            LoteId = loteId,
            GalponId = galponId,
            Notas = notas?.Trim(),
        };
    }

    public void Update(
        string codigo,
        TipoPedido tipo,
        Guid? proveedorId,
        string? descripcion,
        decimal cantidad,
        string? unidad,
        decimal? costoEstimado,
        Guid? loteId,
        Guid? galponId,
        string? notas)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);

        Codigo = codigo.Trim();
        Tipo = tipo;
        ProveedorId = proveedorId;
        Descripcion = descripcion?.Trim();
        Cantidad = cantidad;
        Unidad = unidad?.Trim();
        CostoEstimado = costoEstimado;
        LoteId = loteId;
        GalponId = galponId;
        Notas = notas?.Trim();
    }

    public void Enviar() => Estado = EstadoPedido.Enviado;

    public void Recibir(DateTimeOffset fechaRecepcion, decimal? costoReal)
    {
        Estado = EstadoPedido.Recibido;
        FechaRecepcion = fechaRecepcion;
        if (costoReal.HasValue)
        {
            CostoReal = costoReal;
        }
    }

    public void Cancelar() => Estado = EstadoPedido.Cancelado;
}
