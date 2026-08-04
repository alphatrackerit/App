using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FSH.Modules.Cashflow.Services;

/// <summary>
/// QuestPDF-based proforma renderer. QuestPDF's Community license is free for organisations under
/// $1M USD/year revenue; larger downstream users must obtain a license. The dependency is isolated
/// behind <see cref="IProformaPdfRenderer"/> so it can be swapped without touching callers.
/// User-visible text is Spanish (es-ES) per project convention.
/// </summary>
public sealed class ProformaPdfRenderer : IProformaPdfRenderer
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("es-ES");

    static ProformaPdfRenderer()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Render(ProformaPdfData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(t => t.FontSize(10).FontColor(Colors.Grey.Darken4));

                page.Header().Column(col =>
                {
                    col.Item().Text("PROFORMA").FontSize(22).Bold();
                    col.Item().Text(data.Number).FontSize(12).FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(data.CounterpartyLabel).SemiBold();
                            c.Item().Text(data.CounterpartyName ?? "—");
                            if (!string.IsNullOrWhiteSpace(data.CompanyName))
                            {
                                c.Item().PaddingTop(4).Text("Empresa").SemiBold();
                                c.Item().Text(data.CompanyName);
                            }
                        });
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text($"Tipo: {data.TypeLabel}").SemiBold();
                            c.Item().Text($"Fecha: {FormatDate(data.Date)}");
                            if (!string.IsNullOrWhiteSpace(data.StatusName))
                            {
                                c.Item().Text($"Estado: {data.StatusName}");
                            }
                            if (!string.IsNullOrWhiteSpace(data.PaymentTermsCode))
                            {
                                c.Item().Text($"Forma de pago: {data.PaymentTermsCode}");
                            }
                        });
                    });

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cd =>
                        {
                            cd.RelativeColumn(6);
                            cd.RelativeColumn(3);
                        });

                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(1).PaddingVertical(4).Text("Concepto").SemiBold();
                            header.Cell().BorderBottom(1).PaddingVertical(4).AlignRight().Text("Importe").SemiBold();
                        });

                        if (data.TaxBase is { } taxBase)
                        {
                            table.Cell().PaddingVertical(3).Text("Base imponible");
                            table.Cell().PaddingVertical(3).AlignRight().Text(FormatMoney(taxBase));
                        }

                        if (data.Vat is { } vat)
                        {
                            table.Cell().PaddingVertical(3).Text("IVA");
                            table.Cell().PaddingVertical(3).AlignRight().Text(FormatMoney(vat));
                        }

                        table.Cell().BorderTop(1).PaddingVertical(4).Text("Total").Bold();
                        table.Cell().BorderTop(1).PaddingVertical(4).AlignRight().Text(FormatMoney(data.Total)).Bold();
                    });

                    if (data.Milestones.Count > 0)
                    {
                        col.Item().PaddingTop(6).Text("Vencimientos previstos").SemiBold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cd =>
                            {
                                cd.RelativeColumn(2);
                                cd.RelativeColumn(3);
                                cd.RelativeColumn(3);
                            });

                            table.Header(header =>
                            {
                                header.Cell().BorderBottom(1).PaddingVertical(3).Text("%").SemiBold();
                                header.Cell().BorderBottom(1).PaddingVertical(3).Text("Fecha").SemiBold();
                                header.Cell().BorderBottom(1).PaddingVertical(3).AlignRight().Text("Importe").SemiBold();
                            });

                            foreach (var (amount, dueDate, percentage) in data.Milestones)
                            {
                                table.Cell().PaddingVertical(2).Text(percentage.ToString("0.##", Culture) + " %");
                                table.Cell().PaddingVertical(2).Text(dueDate is null ? "Anticipo" : FormatDate(dueDate));
                                table.Cell().PaddingVertical(2).AlignRight().Text(FormatMoney(amount));
                            }
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(data.Notes))
                    {
                        col.Item().PaddingTop(6).Column(c =>
                        {
                            c.Item().Text("Notas").SemiBold();
                            c.Item().Text(data.Notes);
                        });
                    }
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.DefaultTextStyle(s => s.FontSize(8).FontColor(Colors.Grey.Darken1));
                    t.Span("Documento proforma — sin validez fiscal. ");
                    t.Span("Página ");
                    t.CurrentPageNumber();
                    t.Span(" de ");
                    t.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private static string FormatMoney(decimal value) => value.ToString("N2", Culture) + " €";

    private static string FormatDate(DateTime? date) =>
        date is null ? "—" : date.Value.ToString("dd/MM/yyyy", Culture);
}
