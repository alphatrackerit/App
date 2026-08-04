using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Domain;

namespace Cashflow.Tests.Domain;

public sealed class ProformaTests
{
    private static readonly Guid ClientId = Guid.NewGuid();
    private static readonly Guid SupplierId = Guid.NewGuid();

    [Fact]
    public void Issued_Should_SetTypeEmitida_And_ClientSide()
    {
        var proforma = Proforma.Issued(
            "PRO-2026-001", ClientId, 1000m, new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Unspecified));

        proforma.Type.ShouldBe(InvoiceType.Emitida);
        proforma.ClientId.ShouldBe(ClientId);
        proforma.SupplierId.ShouldBeNull();
        proforma.Number.ShouldBe("PRO-2026-001");
        proforma.Total.ShouldBe(1000m);
    }

    [Fact]
    public void Received_Should_SetTypeRecibida_And_SupplierSide()
    {
        var proforma = Proforma.Received("PF-77", SupplierId, 500m);

        proforma.Type.ShouldBe(InvoiceType.Recibida);
        proforma.SupplierId.ShouldBe(SupplierId);
        proforma.ClientId.ShouldBeNull();
    }

    [Fact]
    public void Issued_Should_NormalizeDate_ToMidnightUnspecified()
    {
        var proforma = Proforma.Issued(
            "PRO-1", ClientId, 100m, new DateTime(2026, 3, 5, 14, 30, 0, DateTimeKind.Utc));

        proforma.Date.ShouldBe(new DateTime(2026, 3, 5, 0, 0, 0, DateTimeKind.Unspecified));
        proforma.Date!.Value.Kind.ShouldBe(DateTimeKind.Unspecified);
    }

    [Fact]
    public void Issued_Should_ParsePaymentTerms_And_IgnoreUnknownCodes()
    {
        var known = Proforma.Issued("PRO-1", ClientId, 100m, paymentTerms: "30PP70-60D");
        var unknown = Proforma.Issued("PRO-2", ClientId, 100m, paymentTerms: "GARBAGE");

        known.PaymentTerms.ShouldNotBeNull();
        known.PaymentTerms.Code.ShouldBe("30PP70-60D");
        unknown.PaymentTerms.ShouldBeNull();
    }

    [Fact]
    public void Update_Should_ReplaceEditableFields_But_KeepType()
    {
        var proforma = Proforma.Issued("PRO-1", ClientId, 100m);

        proforma.Update(
            "PRO-1-BIS", 250m, new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Unspecified), ClientId, null,
            null, null, null, 200m, 50m, "60D", null, "notas", "Antonio Martin");

        proforma.Type.ShouldBe(InvoiceType.Emitida);
        proforma.Number.ShouldBe("PRO-1-BIS");
        proforma.Total.ShouldBe(250m);
        proforma.PaymentTerms!.Code.ShouldBe("60D");
        proforma.Notes.ShouldBe("notas");
        proforma.Responsible.ShouldBe("Antonio Martin");
    }

    [Fact]
    public void AttachDocument_Should_BeIndependentOfUpdate()
    {
        var proforma = Proforma.Issued("PRO-1", ClientId, 100m);
        proforma.AttachDocument("bucket/pro-1.pdf");

        proforma.Update(
            "PRO-1", 100m, null, ClientId, null, null, null, null, null, null, null, null, null, null);

        proforma.DocumentPath.ShouldBe("bucket/pro-1.pdf");
    }
}

public sealed class InvoiceProformaLinkTests
{
    [Fact]
    public void LinkProforma_Should_SetAndClear_WithoutTouchingOtherFields()
    {
        var invoice = Invoice.Issued("F-1", Guid.NewGuid(), 100m);
        var proformaId = Guid.NewGuid();

        invoice.LinkProforma(proformaId);
        invoice.ProformaId.ShouldBe(proformaId);

        invoice.LinkProforma(null);
        invoice.ProformaId.ShouldBeNull();
    }

    [Fact]
    public void Update_Should_NotChangeProformaLink()
    {
        var clientId = Guid.NewGuid();
        var invoice = Invoice.Issued("F-1", clientId, 100m);
        var proformaId = Guid.NewGuid();
        invoice.LinkProforma(proformaId);

        invoice.Update(
            "F-1", 100m, null, null, clientId, null, null, null,
            null, null, null, null, null, null, null, null);

        invoice.ProformaId.ShouldBe(proformaId);
    }
}
