using System.Globalization;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FSH.Modules.Cashflow.Services;

/// <summary>One concept line for the PDF detail table.</summary>
public sealed record InvoicePdfItem(string Description, decimal Quantity, decimal UnitPrice, decimal Amount);

/// <summary>Everything the invoice PDF needs, already resolved — the renderer does no data access.
/// <see cref="QrPayload"/>/<see cref="Hash"/> are null for invoices not registered under VERI*FACTU
/// (the QR block and the legal legend only render when present).</summary>
public sealed record InvoicePdfData(
    string Number,
    DateTime? InvoiceDate,
    DateTime? DueDate,
    string? ClientName,
    string? ClientNif,
    string? ClientAddress,
    string? CompanyName,
    string? CompanyNif,
    string? CompanyAddressLine,
    string? CompanyContactLine,
    byte[]? LogoPng,
    IReadOnlyList<InvoicePdfItem> Items,
    decimal? TaxBase,
    decimal? Vat,
    decimal Total,
    string? PaymentTermsCode,
    string? Notes,
    string? QrPayload,
    string? Hash);

/// <summary>Renders an invoice as a presentable PDF: logo + emitter/client blocks + concept lines
/// + totals; when the invoice is VERI*FACTU-registered, adds the AEAT cotejo QR ("QR tributario:",
/// legend below, per Orden HAC/1177/2024) and the huella. QuestPDF Community license.</summary>
public interface ICashflowInvoicePdfRenderer
{
    byte[] Render(InvoicePdfData data);
}

public sealed class CashflowInvoicePdfRenderer : ICashflowInvoicePdfRenderer
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("es-ES");

    /// <summary>Leyenda obligatoria en factura para sistemas VERI*FACTU (Orden HAC/1177/2024).</summary>
    private const string LegalLegend = "Factura verificable en la sede electrónica de la AEAT";

    static CashflowInvoicePdfRenderer()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Render(InvoicePdfData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        byte[]? qrPng = data.QrPayload is null ? null : RenderQrPng(data.QrPayload);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(t => t.FontSize(10).FontColor(Colors.Grey.Darken4));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        if (data.LogoPng is { Length: > 0 })
                        {
                            col.Item().MaxHeight(60).AlignLeft().Image(data.LogoPng).FitHeight();
                            col.Item().PaddingTop(6);
                        }
                        col.Item().Text("FACTURA").FontSize(22).Bold();
                        col.Item().Text(data.Number).FontSize(12).FontColor(Colors.Grey.Darken1);
                    });
                    if (qrPng is not null)
                    {
                        // QR AEAT en primera página/cabecera, ~30-40 mm (ISO/IEC 18004, nivel M),
                        // precedido de «QR tributario:» y con la leyenda legal justo debajo.
                        row.ConstantItem(110).Column(col =>
                        {
                            col.Item().Text("QR tributario:").FontSize(8);
                            col.Item().Width(95).Image(qrPng);
                            col.Item().Width(95).Text(LegalLegend).FontSize(7).SemiBold();
                        });
                    }
                });

                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Emisor").SemiBold();
                            c.Item().Text(data.CompanyName ?? "—");
                            if (!string.IsNullOrWhiteSpace(data.CompanyNif))
                            {
                                c.Item().Text($"NIF: {data.CompanyNif}");
                            }
                            if (!string.IsNullOrWhiteSpace(data.CompanyAddressLine))
                            {
                                c.Item().Text(data.CompanyAddressLine).FontColor(Colors.Grey.Darken1);
                            }
                            if (!string.IsNullOrWhiteSpace(data.CompanyContactLine))
                            {
                                c.Item().Text(data.CompanyContactLine).FontColor(Colors.Grey.Darken1);
                            }
                        });
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text("Cliente").SemiBold();
                            c.Item().Text(data.ClientName ?? "—");
                            if (!string.IsNullOrWhiteSpace(data.ClientNif))
                            {
                                c.Item().Text($"NIF: {data.ClientNif}");
                            }
                            if (!string.IsNullOrWhiteSpace(data.ClientAddress))
                            {
                                c.Item().Text(data.ClientAddress).FontColor(Colors.Grey.Darken1);
                            }
                        });
                    });

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"Fecha de expedición: {FormatDate(data.InvoiceDate)}");
                        row.RelativeItem().AlignRight().Text($"Vencimiento: {FormatDate(data.DueDate)}");
                    });

                    if (data.Items.Count > 0)
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cd =>
                            {
                                cd.RelativeColumn(6);
                                cd.RelativeColumn(2);
                                cd.RelativeColumn(2);
                                cd.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().BorderBottom(1).PaddingVertical(4).Text("Concepto").SemiBold();
                                header.Cell().BorderBottom(1).PaddingVertical(4).AlignRight().Text("Cantidad").SemiBold();
                                header.Cell().BorderBottom(1).PaddingVertical(4).AlignRight().Text("Precio").SemiBold();
                                header.Cell().BorderBottom(1).PaddingVertical(4).AlignRight().Text("Importe").SemiBold();
                            });

                            foreach (var item in data.Items)
                            {
                                table.Cell().PaddingVertical(3).Text(item.Description);
                                table.Cell().PaddingVertical(3).AlignRight().Text(item.Quantity.ToString("0.##", Culture));
                                table.Cell().PaddingVertical(3).AlignRight().Text(FormatMoney(item.UnitPrice));
                                table.Cell().PaddingVertical(3).AlignRight().Text(FormatMoney(item.Amount));
                            }
                        });
                    }

                    col.Item().AlignRight().MaxWidth(220).Table(table =>
                    {
                        table.ColumnsDefinition(cd =>
                        {
                            cd.RelativeColumn(5);
                            cd.RelativeColumn(4);
                        });

                        if (data.TaxBase is { } taxBase)
                        {
                            table.Cell().PaddingVertical(2).Text("Base imponible");
                            table.Cell().PaddingVertical(2).AlignRight().Text(FormatMoney(taxBase));
                        }

                        if (data.Vat is { } vat)
                        {
                            table.Cell().PaddingVertical(2).Text("IVA");
                            table.Cell().PaddingVertical(2).AlignRight().Text(FormatMoney(vat));
                        }

                        table.Cell().BorderTop(1).PaddingVertical(3).Text("Total").Bold();
                        table.Cell().BorderTop(1).PaddingVertical(3).AlignRight().Text(FormatMoney(data.Total)).Bold();
                    });

                    if (!string.IsNullOrWhiteSpace(data.PaymentTermsCode))
                    {
                        col.Item().Text($"Forma de pago: {data.PaymentTermsCode}").FontColor(Colors.Grey.Darken1);
                    }

                    if (!string.IsNullOrWhiteSpace(data.Notes))
                    {
                        col.Item().PaddingTop(4).Column(c =>
                        {
                            c.Item().Text("Notas").SemiBold();
                            c.Item().Text(data.Notes);
                        });
                    }
                });

                if (data.Hash is not null)
                {
                    page.Footer().AlignCenter()
                        .Text($"VERI*FACTU · Huella: {data.Hash}")
                        .FontSize(6.5f).FontColor(Colors.Grey.Darken1);
                }
            });
        }).GeneratePdf();
    }

    private static byte[] RenderQrPng(string payload)
    {
        using var generator = new QRCodeGenerator();
        using var qrData = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
        using var png = new PngByteQRCode(qrData);
        return png.GetGraphic(pixelsPerModule: 6);
    }

    private static string FormatMoney(decimal value) => value.ToString("N2", Culture) + " €";

    private static string FormatDate(DateTime? date) =>
        date is null ? "—" : date.Value.ToString("dd/MM/yyyy", Culture);
}
