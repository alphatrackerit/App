using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Invoices;

/// <summary>
/// Attaches (or replaces) the digitalized source document of an EXISTING invoice — the record may
/// have been created before the paper was scanned. Returns the stored document path.
/// </summary>
public sealed record AttachInvoiceDocumentCommand(
    Guid InvoiceId,
    byte[] Content,
    string FileName,
    string ContentType) : ICommand<string>;
