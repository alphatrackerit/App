using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Proformas;

/// <summary>Renders the proforma as a commercial PDF document (no QR/hash — a proforma is not a
/// fiscal document and never enters the VeriFactu engine).</summary>
public sealed record GetProformaPdfQuery(Guid ProformaId) : IQuery<ProformaPdfDto>;

public sealed record ProformaPdfDto(byte[] Content, string FileName);
