using FSH.Framework.Core.Domain;
using FSH.Modules.Avicola.Contracts;

namespace FSH.Modules.Avicola.Domain;

/// <summary>
/// A document/attachment tied to an Avícola entity (flock, shed, order, movement). The file itself
/// is managed by the Files module via the presigned-upload flow; this entity stores the durable
/// reference (<see cref="FileAssetId"/> + <see cref="Url"/>) plus business metadata.
/// </summary>
public sealed class Documento : AggregateRoot<Guid>
{
    public TipoDocumento Tipo { get; private set; }
    public DocumentoOrigen Origen { get; private set; }

    /// <summary>Id of the owning entity (flock/shed/order/movement). Null for <c>General</c>.</summary>
    public Guid? OrigenId { get; private set; }

    /// <summary>Files-module FileAsset id (best-effort cleanup on delete). Null for external URLs.</summary>
    public Guid? FileAssetId { get; private set; }

    /// <summary>Durable URL captured at finalize time.</summary>
    public string Url { get; private set; } = default!;

    public string NombreArchivo { get; private set; } = default!;
    public string? ContentType { get; private set; }
    public DateTimeOffset Fecha { get; private set; }
    public string? Notas { get; private set; }

    private Documento() { }

    public static Documento Create(
        TipoDocumento tipo,
        DocumentoOrigen origen,
        Guid? origenId,
        Guid? fileAssetId,
        string url,
        string nombreArchivo,
        string? contentType,
        DateTimeOffset fecha,
        string? notas)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentException.ThrowIfNullOrWhiteSpace(nombreArchivo);

        return new Documento
        {
            Id = Guid.CreateVersion7(),
            Tipo = tipo,
            Origen = origen,
            OrigenId = origenId,
            FileAssetId = fileAssetId,
            Url = url.Trim(),
            NombreArchivo = nombreArchivo.Trim(),
            ContentType = contentType?.Trim(),
            Fecha = fecha,
            Notas = notas?.Trim(),
        };
    }

    public void Update(TipoDocumento tipo, string? notas)
    {
        Tipo = tipo;
        Notas = notas?.Trim();
    }
}
