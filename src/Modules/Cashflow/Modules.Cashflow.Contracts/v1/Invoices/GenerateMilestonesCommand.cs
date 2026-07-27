using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Invoices;

/// <summary>
/// Materialises the invoice's <c>FormaPago</c> milestones as forecast cash lines (§5): N Incomes for
/// an <c>Emitida</c> invoice, N Payments for a <c>Recibida</c> one. Each line is born
/// <c>Confirmed=false, Validated=false</c> and linked to the invoice. Optionally assigns a project
/// to every generated line. Fails if the invoice already has linked lines (idempotency guard).
/// Returns the ids of the created lines.
/// </summary>
public sealed record GenerateMilestonesCommand(
    Guid InvoiceId,
    Guid? ProjectId = null) : ICommand<IReadOnlyList<Guid>>;
