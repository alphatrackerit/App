namespace FSH.Modules.Avicola.Contracts;

/// <summary>Lifecycle of a broiler flock (lote de engorde).</summary>
public enum EstadoLote
{
    /// <summary>Planned — chicks not yet placed.</summary>
    Planificado,

    /// <summary>Active rearing — chicks placed, growing.</summary>
    EnCrianza,

    /// <summary>Closed — birds harvested/dispatched, flock finished.</summary>
    Finalizado,

    /// <summary>Cancelled before completion.</summary>
    Cancelado,
}

/// <summary>Feed phase fed to a flock (programa de alimentación).</summary>
public enum TipoAlimento
{
    /// <summary>Starter feed (pre-iniciador / iniciador).</summary>
    Iniciador,

    /// <summary>Grower feed (crecimiento).</summary>
    Crecimiento,

    /// <summary>Finisher feed (engorde).</summary>
    Engorde,

    /// <summary>Withdrawal feed (final / retiro).</summary>
    Final,
}

/// <summary>Kind of health/biosecurity record applied to a flock.</summary>
public enum TipoRegistroSanitario
{
    /// <summary>Vaccination.</summary>
    Vacunacion,

    /// <summary>Medication (e.g. antibiotics).</summary>
    Medicacion,

    /// <summary>General treatment.</summary>
    Tratamiento,

    /// <summary>Vitamins / supplements.</summary>
    Vitaminas,
}
