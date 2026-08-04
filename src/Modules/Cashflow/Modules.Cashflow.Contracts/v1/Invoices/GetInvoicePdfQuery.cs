using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Invoices;

/// <summary>Renders the invoice as a presentable PDF (logo, emitter/client blocks, concept lines,
/// totals). When the invoice is VERI*FACTU-registered the PDF also carries the AEAT QR + legal
/// legend + huella. On-demand, never persisted. Named "Cashflow" because endpoint names are global
/// and Billing owns the plain invoice-PDF names.</summary>
public sealed record GetCashflowInvoicePdfQuery(Guid InvoiceId) : IQuery<InvoicePdfDto>;

public sealed record InvoicePdfDto(byte[] Content, string FileName);
