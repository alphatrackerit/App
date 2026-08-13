using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Domain;

namespace Cashflow.Tests.Domain;

/// <summary>Draft (numberless) received invoices: Number is nullable for Recibidas only.</summary>
public sealed class InvoiceNumberTests
{
    private static readonly Guid ClientId = Guid.NewGuid();
    private static readonly Guid SupplierId = Guid.NewGuid();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Received_Should_AllowMissingNumber_AsNull(string? number)
    {
        var invoice = Invoice.Received(number, SupplierId, 100m);

        invoice.Number.ShouldBeNull();
        invoice.Type.ShouldBe(InvoiceType.Recibida);
    }

    [Fact]
    public void Received_Should_TrimNumber_WhenProvided()
    {
        var invoice = Invoice.Received("  FR-9 ", SupplierId, 100m);

        invoice.Number.ShouldBe("FR-9");
    }

    [Fact]
    public void Issued_Should_Throw_WithoutNumber()
    {
        Should.Throw<ArgumentException>(() => Invoice.Issued(" ", ClientId, 100m));
    }

    [Fact]
    public void Update_Should_Throw_WhenEmitida_LosesNumber()
    {
        var invoice = Invoice.Issued("FA-1", ClientId, 100m);

        Should.Throw<ArgumentException>(() => invoice.Update(
            null, 100m, null, null, ClientId, null, null, null, null, null, null, null, null, null, null, null));
    }

    [Fact]
    public void Update_Should_AllowRecibida_ToClearNumber()
    {
        var invoice = Invoice.Received("FR-1", SupplierId, 100m);

        invoice.Update(
            "", 100m, null, null, null, SupplierId, null, null, null, null, null, null, null, null, null, null);

        invoice.Number.ShouldBeNull();
    }

    [Fact]
    public void Update_Should_AllowRecibida_ToSetNumberLater()
    {
        var invoice = Invoice.Received(null, SupplierId, 100m);

        invoice.Update(
            "FR-2026-15", 100m, null, null, null, SupplierId, null, null, null, null, null, null, null, null, null, null);

        invoice.Number.ShouldBe("FR-2026-15");
    }
}
