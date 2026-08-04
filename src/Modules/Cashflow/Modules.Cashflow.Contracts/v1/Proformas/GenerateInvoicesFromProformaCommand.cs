using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Proformas;

/// <summary>
/// Materialises the proforma's <c>PaymentTerms</c> milestones as N draft invoices (one per
/// instalment, numbered <c>{proforma.Number}-{i}</c>, inheriting the proforma's counterparty /
/// company / society / project). Unlike invoice milestone generation this does NOT refuse when
/// invoices are already linked — automatic generation and manual linking are meant to combine;
/// a derived-number collision surfaces as 409 instead.
/// </summary>
public sealed record GenerateInvoicesFromProformaCommand(Guid ProformaId) : ICommand<IReadOnlyList<Guid>>;
