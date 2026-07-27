using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Domain;
using FSH.Modules.Cashflow.Features.v1.Invoices.GetLinkSuggestions;
using static FSH.Modules.Cashflow.Features.v1.Invoices.GetLinkSuggestions.LinkSuggestionMatcher;

namespace Cashflow.Tests.Features;

public sealed class LinkSuggestionMatcherTests
{
    private static readonly Guid Supplier = Guid.NewGuid();
    private static readonly Guid OtherSupplier = Guid.NewGuid();
    private static readonly DateTime Date = new(2026, 3, 1, 0, 0, 0, DateTimeKind.Unspecified);

    private static InvoiceCandidate Invoice(decimal total, Guid? counterparty = null, string? terms = null, string number = "F-1") =>
        new(Guid.NewGuid(), number, InvoiceType.Recibida, total, counterparty ?? Supplier, Date,
            terms is null ? null : PaymentTerms.Parse(terms));

    private static LineCandidate Line(decimal amount, Guid? counterparty = null) =>
        new(Guid.NewGuid(), amount, Date, counterparty ?? Supplier, null, null);

    [Fact]
    public void Compute_Should_MatchWholeTotal_When_ExactlyOneCandidateSharesCounterpartyAndAmount()
    {
        var invoice = Invoice(1000m);
        var line = Line(1000m);

        var result = Compute([invoice], [line, Line(500m), Line(1000m, OtherSupplier)]);

        result.Matches.Count.ShouldBe(1);
        result.Matches[0].Line.Id.ShouldBe(line.Id);
        result.Matches[0].Reason.ShouldBe("IMPORTE EXACTO + CONTRAPARTE");
        result.AmbiguousInvoices.ShouldBe(0);
    }

    [Fact]
    public void Compute_Should_ReportAmbiguous_When_TwoLinesMatchTheSameTotal()
    {
        var invoice = Invoice(1000m);

        var result = Compute([invoice], [Line(1000m), Line(1000m)]);

        result.Matches.ShouldBeEmpty();
        result.AmbiguousInvoices.ShouldBe(1);
    }

    [Fact]
    public void Compute_Should_MatchMilestoneSplit_When_EachSlotHasExactlyOneLine()
    {
        // 30PP70-60D on 1000 → 300 + 700
        var invoice = Invoice(1000m, terms: "30PP70-60D");
        var l30 = Line(300m);
        var l70 = Line(700m);

        var result = Compute([invoice], [l30, l70, Line(42m)]);

        result.Matches.Count.ShouldBe(2);
        result.Matches.Select(m => m.Line.Id).ShouldBe([l30.Id, l70.Id], ignoreOrder: true);
        result.Matches.ShouldAllBe(m => m.Reason.Contains("30PP70-60D"));
    }

    [Fact]
    public void Compute_Should_NotSuggestPartialMilestones_When_ASlotHasNoLine()
    {
        var invoice = Invoice(1000m, terms: "30PP70-60D");

        var result = Compute([invoice], [Line(300m)]); // 700 missing

        result.Matches.ShouldBeEmpty();
        result.AmbiguousInvoices.ShouldBe(0); // not ambiguous — just unmatched
    }

    [Fact]
    public void Compute_Should_MatchNegativePairs_When_CreditNote()
    {
        var abono = Invoice(-567.05m, number: "NA-1");
        var neg = Line(-567.05m);

        var result = Compute([abono], [neg, Line(567.05m)]);

        result.Matches.Count.ShouldBe(1);
        result.Matches[0].Line.Id.ShouldBe(neg.Id);
    }

    [Fact]
    public void Compute_Should_DropBothInvoices_When_TheyContestTheSameLine()
    {
        // Two invoices, same supplier, same total, ONE matching line: the line is contested.
        var a = Invoice(1000m, number: "F-A");
        var b = Invoice(1000m, number: "F-B");
        var line = Line(1000m);

        var result = Compute([a, b], [line]);

        result.Matches.ShouldBeEmpty();
        result.AmbiguousInvoices.ShouldBe(2);
    }

    [Fact]
    public void Compute_Should_IgnoreLinesAndInvoicesWithoutCounterparty()
    {
        var noCounterparty = Invoice(1000m) with { CounterpartyId = null };
        var orphanLine = Line(1000m) with { CounterpartyId = null };

        var result = Compute([noCounterparty, Invoice(500m)], [orphanLine, Line(500m)]);

        result.Matches.Count.ShouldBe(1);
        result.Matches[0].Invoice.Total.ShouldBe(500m);
    }

    [Fact]
    public void Compute_Should_UseTolerance_When_AmountsDifferByLessThanACent()
    {
        var invoice = Invoice(100.005m);

        var result = Compute([invoice], [Line(100.01m)]);

        result.Matches.Count.ShouldBe(1);
    }
}
