using FSH.Modules.Cashflow.Domain;

namespace Cashflow.Tests.Domain;

public sealed class PaymentTermsTests
{
    private static readonly DateTime Issue = new(2026, 1, 10, 0, 0, 0, DateTimeKind.Unspecified);

    #region Parse — the eight grammar codes (§5)

    [Theory]
    [InlineData("0D", 0)]
    [InlineData("30D", 30)]
    [InlineData("60D", 60)]
    public void Parse_Should_ProduceSingleInvoiceDateMilestone_When_NDCode(string code, int offset)
    {
        var terms = PaymentTerms.Parse(code);

        terms.Milestones.Count.ShouldBe(1);
        terms.Milestones[0].Percentage.ShouldBe(100m);
        terms.Milestones[0].Trigger.ShouldBe(PaymentTrigger.InvoiceDate);
        terms.Milestones[0].OffsetDays.ShouldBe(offset);
        terms.IsDirectDebit.ShouldBeFalse();
    }

    [Fact]
    public void Parse_Should_ProduceSinglePrepaidMilestone_When_100PP()
    {
        var terms = PaymentTerms.Parse("100PP");

        terms.Milestones.Count.ShouldBe(1);
        terms.Milestones[0].Percentage.ShouldBe(100m);
        terms.Milestones[0].Trigger.ShouldBe(PaymentTrigger.Prepaid);
    }

    [Fact]
    public void Parse_Should_ProducePrepaidThenOffset_When_30PP70_60D()
    {
        var terms = PaymentTerms.Parse("30PP70-60D");

        terms.Milestones.Count.ShouldBe(2);
        terms.Milestones[0].ShouldBe(new PaymentMilestone(30m, PaymentTrigger.Prepaid, 0));
        terms.Milestones[1].ShouldBe(new PaymentMilestone(70m, PaymentTrigger.InvoiceDate, 60));
    }

    [Fact]
    public void Parse_Should_ProducePrepaidThenOffset_When_50PP50_60D()
    {
        var terms = PaymentTerms.Parse("50PP50-60D");

        terms.Milestones[0].ShouldBe(new PaymentMilestone(50m, PaymentTrigger.Prepaid, 0));
        terms.Milestones[1].ShouldBe(new PaymentMilestone(50m, PaymentTrigger.InvoiceDate, 60));
    }

    [Fact]
    public void Parse_Should_ProduceTwoPrepaid_When_50PP50PP()
    {
        var terms = PaymentTerms.Parse("50PP50PP");

        terms.Milestones.Count.ShouldBe(2);
        terms.Milestones[0].Trigger.ShouldBe(PaymentTrigger.Prepaid);
        terms.Milestones[1].Trigger.ShouldBe(PaymentTrigger.Prepaid);
    }

    [Fact]
    public void Parse_Should_FlagDirectDebit_When_Domiciliado()
    {
        var terms = PaymentTerms.Parse("DOMICILIADO");

        terms.IsDirectDebit.ShouldBeTrue();
        terms.Milestones.Count.ShouldBe(1);
        terms.Milestones[0].Percentage.ShouldBe(100m);
        terms.Milestones[0].Trigger.ShouldBe(PaymentTrigger.InvoiceDate);
    }

    #endregion

    #region Parse — normalization and rejection

    [Theory]
    [InlineData("  30pp70-60d  ")]
    [InlineData("30PP70-60D")]
    public void Parse_Should_BeCaseInsensitiveAndTrimmed(string raw)
    {
        var terms = PaymentTerms.Parse(raw);

        terms.Code.ShouldBe("30PP70-60D");
    }

    [Theory]
    [InlineData("BOGUS")]
    [InlineData("30X")]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_Should_Throw_When_CodeUnrecognized(string raw)
    {
        Should.Throw<Exception>(() => PaymentTerms.Parse(raw));
    }

    [Fact]
    public void Parse_Should_Throw_When_PercentagesDoNotSumTo100()
    {
        // 30 + 60 = 90 → rejected (§5 domain rule).
        Should.Throw<FormatException>(() => PaymentTerms.Parse("30PP60-60D"));
    }

    [Theory]
    [InlineData("30PP70-60D", true)]
    [InlineData("BOGUS", false)]
    [InlineData(null, false)]
    [InlineData("", false)]
    public void TryParse_Should_ReturnSuccessFlag(string? raw, bool expected)
    {
        PaymentTerms.TryParse(raw, out var terms).ShouldBe(expected);
        (terms is not null).ShouldBe(expected);
    }

    #endregion

    #region GenerateMilestones

    [Fact]
    public void GenerateMilestones_Should_SplitTotalAndComputeDueDates_When_PrepaidThenOffset()
    {
        var terms = PaymentTerms.Parse("30PP70-60D");

        var result = terms.GenerateMilestones(1000m, Issue).ToList();

        result.Count.ShouldBe(2);
        result[0].Amount.ShouldBe(300m);
        result[0].DueDate.ShouldBeNull();                 // prepaid → no due date
        result[1].Amount.ShouldBe(700m);
        result[1].DueDate.ShouldBe(Issue.AddDays(60));
    }

    [Fact]
    public void GenerateMilestones_Should_AlwaysReSumToTotal_EvenWithRoundingRemainder()
    {
        var terms = PaymentTerms.Parse("30PP70-60D");

        var result = terms.GenerateMilestones(100.01m, Issue).ToList();

        result.Sum(r => r.Amount).ShouldBe(100.01m);       // last milestone absorbs the remainder
    }

    [Fact]
    public void GenerateMilestones_Should_YieldNullDueDate_When_InvoiceDateMissing()
    {
        var terms = PaymentTerms.Parse("60D");

        var result = terms.GenerateMilestones(500m, invoiceDate: null).ToList();

        result.Count.ShouldBe(1);
        result[0].Amount.ShouldBe(500m);
        result[0].DueDate.ShouldBeNull();
    }

    [Fact]
    public void GenerateMilestones_Should_UseInvoiceDatePlusOffset_When_SingleOffset()
    {
        var terms = PaymentTerms.Parse("30D");

        var result = terms.GenerateMilestones(500m, Issue).ToList();

        result[0].DueDate.ShouldBe(Issue.AddDays(30));
    }

    #endregion
}
