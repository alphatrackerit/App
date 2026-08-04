using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Proformas;

/// <summary>Attaches (or replaces) the source document of an existing proforma. Returns the stored
/// document path.</summary>
public sealed record AttachProformaDocumentCommand(
    Guid ProformaId,
    byte[] Content,
    string FileName,
    string ContentType) : ICommand<string>;
