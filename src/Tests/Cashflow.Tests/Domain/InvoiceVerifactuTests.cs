using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Domain;

namespace Cashflow.Tests.Domain;

public sealed class InvoiceVerifactuTests
{
    private static Invoice NewIssued() => Invoice.Issued("F-1", Guid.NewGuid(), 121m, vat: 21m, taxBase: 100m);

    [Fact]
    public void Invoice_Should_StartAsNoAplica()
    {
        NewIssued().VerifactuStatus.ShouldBe(VerifactuStatus.NoAplica);
    }

    [Fact]
    public void MarkVerifactuIssued_Should_MoveToPendienteEnvio()
    {
        var invoice = NewIssued();
        invoice.MarkVerifactuIssued();
        invoice.VerifactuStatus.ShouldBe(VerifactuStatus.PendienteEnvio);
    }

    [Theory]
    [InlineData(VerifactuStatus.PendienteEnvio)]
    [InlineData(VerifactuStatus.Enviada)]
    [InlineData(VerifactuStatus.Aceptada)]
    [InlineData(VerifactuStatus.AceptadaConErrores)]
    public void Update_Should_Throw409_WhenRegistered(VerifactuStatus status)
    {
        var invoice = NewIssued();
        invoice.MarkVerifactuIssued();
        invoice.MarkVerifactuResult(status);

        var ex = Should.Throw<CustomException>(() =>
            invoice.Update("F-1", 121m, null, null, invoice.ClientId, null, null, null, 100m, 21m, null, null, null, null, null, null));
        ex.StatusCode.ShouldBe(System.Net.HttpStatusCode.Conflict);
    }

    [Fact]
    public void Update_Should_Work_WhenRechazada()
    {
        // A rejected record never registered anything at the AEAT — the invoice stays editable.
        var invoice = NewIssued();
        invoice.MarkVerifactuIssued();
        invoice.MarkVerifactuResult(VerifactuStatus.Rechazada);

        invoice.Update("F-1-BIS", 121m, null, null, invoice.ClientId, null, null, null, 100m, 21m, null, null, null, null, null, null);
        invoice.Number.ShouldBe("F-1-BIS");
    }

    [Fact]
    public void VerifactuRecord_Should_TrackRetries_OnTechnicalError()
    {
        var record = InvoiceVerifactuRecord.Create(Guid.NewGuid(), null, new string('A', 64), DateTimeOffset.UnixEpoch, "https://x/qr");

        record.Status.ShouldBe(VerifactuStatus.PendienteEnvio);
        record.MarkTechnicalError();
        record.MarkTechnicalError();
        record.RetryCount.ShouldBe(2);
        record.Status.ShouldBe(VerifactuStatus.ErrorTecnico);
    }
}
