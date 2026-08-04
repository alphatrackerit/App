using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Services;

namespace Cashflow.Tests.Services;

public sealed class VerifactuChainServiceTests
{
    private readonly VerifactuChainService _service = new();

    // Official AEAT test vector — "Detalle de las especificaciones técnicas para generación de la
    // huella o hash de los registros de facturación" v0.1.2 (27/08/2024), Caso 1: first record,
    // empty previous Huella.
    private static readonly VerifactuChainInput OfficialCase1 = new(
        EmitterNif: "89890001K",
        InvoiceNumber: "12345678/G33",
        IssueDate: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
        VatAmount: 12.35m,
        TotalAmount: 123.45m,
        PreviousHash: null,
        GeneratedAt: new DateTimeOffset(2024, 1, 1, 19, 20, 30, TimeSpan.FromHours(1)));

    private const string OfficialCase1Hash = "3C464DAF61ACB827C65FDA19F352A4E3BDC2C640E9E9FC4CC058073F38F12F60";
    private const string OfficialCase2Hash = "F7B94CFD8924EDFF273501B01EE5153E4CE8F259766F88CF6ACB8935802A2B97";

    [Fact]
    public void BuildRecord_Should_MatchOfficialAeatVector_Case1_FirstRecord()
    {
        var result = _service.BuildRecord(OfficialCase1, VerifactuEnvironment.Pruebas);

        result.CanonicalInput.ShouldBe(
            "IDEmisorFactura=89890001K&NumSerieFactura=12345678/G33&FechaExpedicionFactura=01-01-2024" +
            "&TipoFactura=F1&CuotaTotal=12.35&ImporteTotal=123.45&Huella=" +
            "&FechaHoraHusoGenRegistro=2024-01-01T19:20:30+01:00");
        result.Hash.ShouldBe(OfficialCase1Hash);
    }

    [Fact]
    public void BuildRecord_Should_MatchOfficialAeatVector_Case2_ChainedRecord()
    {
        // Official Caso 2: second record — next invoice number, +5 s, chains Caso 1's hash.
        var input = OfficialCase1 with
        {
            InvoiceNumber = "12345679/G34",
            PreviousHash = OfficialCase1Hash,
            GeneratedAt = new DateTimeOffset(2024, 1, 1, 19, 20, 35, TimeSpan.FromHours(1)),
        };

        var result = _service.BuildRecord(input, VerifactuEnvironment.Pruebas);

        result.Hash.ShouldBe(OfficialCase2Hash);
    }

    [Fact]
    public void BuildRecord_Should_BeDeterministic_And_SensitiveToPreviousHash()
    {
        var a1 = _service.BuildRecord(OfficialCase1, VerifactuEnvironment.Pruebas);
        var a2 = _service.BuildRecord(OfficialCase1, VerifactuEnvironment.Pruebas);
        var b = _service.BuildRecord(OfficialCase1 with { PreviousHash = a1.Hash }, VerifactuEnvironment.Pruebas);

        a1.Hash.ShouldBe(a2.Hash);
        b.Hash.ShouldNotBe(a1.Hash);
        a1.Hash.Length.ShouldBe(64);
        a1.Hash.ShouldBe(a1.Hash.ToUpperInvariant());
    }

    [Fact]
    public void BuildRecord_Should_TargetProperCotejoHost_PerEnvironment()
    {
        var pruebas = _service.BuildRecord(OfficialCase1, VerifactuEnvironment.Pruebas);
        var produccion = _service.BuildRecord(OfficialCase1, VerifactuEnvironment.Produccion);

        pruebas.QrPayload.ShouldStartWith("https://prewww2.aeat.es/wlpl/TIKE-CONT/ValidarQR?");
        produccion.QrPayload.ShouldStartWith("https://www2.agenciatributaria.gob.es/wlpl/TIKE-CONT/ValidarQR?");
    }

    [Fact]
    public void BuildRecord_Should_UrlEncodeQrParams_ButNotTheHashInput()
    {
        // AEAT QR spec: values URL-encoded in the QR URL; the hash canonical string is NOT encoded.
        var result = _service.BuildRecord(OfficialCase1, VerifactuEnvironment.Pruebas);

        result.QrPayload.ShouldContain("numserie=12345678%2FG33");
        result.QrPayload.ShouldContain("fecha=01-01-2024");
        result.QrPayload.ShouldContain("importe=123.45");
        result.CanonicalInput.ShouldContain("NumSerieFactura=12345678/G33");
    }
}
