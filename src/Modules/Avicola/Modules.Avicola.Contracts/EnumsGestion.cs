namespace FSH.Modules.Avicola.Contracts;

/// <summary>Kind of document/attachment stored against an Avícola entity.</summary>
public enum TipoDocumento
{
    /// <summary>Feed delivery note (albarán de pienso).</summary>
    AlbaranPienso,

    /// <summary>Shed cleaning certificate (certificado de limpieza de la nave).</summary>
    CertificadoLimpieza,

    /// <summary>Mortality supporting document.</summary>
    Mortalidad,

    /// <summary>Health / sanitary document (vaccination record, vet report).</summary>
    Sanidad,

    /// <summary>Document tied to a purchase order.</summary>
    Pedido,

    /// <summary>Invoice / receipt.</summary>
    Factura,

    /// <summary>Any other document.</summary>
    Otro,
}

/// <summary>The kind of Avícola entity a document is attached to.</summary>
public enum DocumentoOrigen
{
    Lote,
    Galpon,
    Pedido,
    Movimiento,
    Preparacion,
    General,
}

/// <summary>State of a between-flocks shed preparation (vacío sanitario).</summary>
public enum EstadoPreparacion
{
    EnProceso,
    Completada,
}

/// <summary>What a purchase order is for.</summary>
public enum TipoPedido
{
    Pienso,
    Pollitos,
    Medicamento,
    Insumo,
    Otro,
}

/// <summary>Lifecycle of a purchase order.</summary>
public enum EstadoPedido
{
    Borrador,
    Enviado,
    Recibido,
    Cancelado,
}

/// <summary>A lifecycle transition applied to a purchase order.</summary>
public enum AccionPedido
{
    Enviar,
    Recibir,
    Cancelar,
}

/// <summary>Direction of an accounting movement.</summary>
public enum TipoMovimiento
{
    Ingreso,
    Egreso,
}

/// <summary>Accounting category for a movement / cost roll-up.</summary>
public enum CategoriaMovimiento
{
    Pollitos,
    Pienso,
    Sanidad,
    ManoDeObra,
    Servicios,
    Transporte,
    VentaPollos,
    Otro,
}
