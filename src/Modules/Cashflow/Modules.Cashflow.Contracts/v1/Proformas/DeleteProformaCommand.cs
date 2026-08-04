using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Proformas;

/// <summary>Deletes a proforma. Linked invoices survive — their <c>ProformaId</c> is set to null
/// by the database (ON DELETE SET NULL), same rule as deleting an invoice with cash lines.</summary>
public sealed record DeleteProformaCommand(Guid ProformaId) : ICommand<Unit>;
