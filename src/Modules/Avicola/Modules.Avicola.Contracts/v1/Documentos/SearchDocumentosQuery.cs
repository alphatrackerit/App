using FSH.Framework.Shared.Persistence;
using FSH.Modules.Avicola.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Documentos;

/// <summary>Search documents (paged), optionally scoped to one owner entity and/or type.</summary>
/// <param name="Origen">Optional owner kind filter (Lote/Galpon/Pedido/Movimiento/General).</param>
/// <param name="OrigenId">Optional owner id filter.</param>
/// <param name="Tipo">Optional document-type filter.</param>
/// <param name="PageNumber">1-based page number.</param>
/// <param name="PageSize">Page size (1-200).</param>
/// <param name="SortBy">Sort column. One of: fecha | nombre.</param>
/// <param name="SortDir">Sort direction. One of: asc | desc.</param>
public sealed record SearchDocumentosQuery(
    DocumentoOrigen? Origen = null,
    Guid? OrigenId = null,
    TipoDocumento? Tipo = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null) : IQuery<PagedResponse<DocumentoDto>>;
