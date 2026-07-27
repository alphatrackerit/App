using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Invoices;

public sealed record DeleteInvoiceCommand(Guid InvoiceId) : ICommand<Unit>;
