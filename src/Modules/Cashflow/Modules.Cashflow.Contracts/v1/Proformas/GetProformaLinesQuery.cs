using FSH.Modules.Cashflow.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Proformas;

/// <summary>Returns the invoices linked to a proforma plus the derived totals (informative cuadre).</summary>
public sealed record GetProformaLinesQuery(Guid ProformaId) : IQuery<ProformaLinesDto>;
