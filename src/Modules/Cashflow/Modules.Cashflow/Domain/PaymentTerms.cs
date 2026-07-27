using System.Globalization;
using System.Text.RegularExpressions;

namespace FSH.Modules.Cashflow.Domain;

/// <summary>When a <see cref="PaymentMilestone"/> falls due.</summary>
public enum PaymentTrigger
{
    /// <summary>Prepaid — due at (or before) issue; no date offset applies.</summary>
    Prepaid,

    /// <summary>Relative to the invoice date, shifted by <see cref="PaymentMilestone.OffsetDays"/>.</summary>
    InvoiceDate,
}

/// <summary>One instalment of a <see cref="PaymentTerms"/>: a percentage of the invoice total, a
/// trigger, and (for <see cref="PaymentTrigger.InvoiceDate"/>) a day offset.</summary>
public sealed record PaymentMilestone(decimal Percentage, PaymentTrigger Trigger, int OffsetDays);

/// <summary>
/// Value object that turns a payment-terms code ("30PP70-60D", "60D", "DOMICILIADO"…) into the
/// concrete instalments (milestones) it generates. This is what lets the module project future
/// due dates and materialise them as forecast <c>Income</c>/<c>Payment</c> rows (§5 of the plan).
/// Persisted as its <see cref="Code"/> string via an EF value converter; the milestones are derived,
/// never stored.
/// </summary>
public sealed partial class PaymentTerms
{
    private const decimal FullPercentage = 100m;
    private const decimal Tolerance = 0.01m;

    private PaymentTerms(string code, IReadOnlyList<PaymentMilestone> milestones, bool isDirectDebit)
    {
        Code = code;
        Milestones = milestones;
        IsDirectDebit = isDirectDebit;
    }

    /// <summary>The raw, normalized code (upper-case, trimmed) — the persisted representation.</summary>
    public string Code { get; }

    public IReadOnlyList<PaymentMilestone> Milestones { get; }

    /// <summary>True for <c>DOMICILIADO</c>: collection is direct-debited on the due date.</summary>
    public bool IsDirectDebit { get; }

    // One regex per alternative (no single monster pattern), per §5.
    [GeneratedRegex(@"^(\d+)D$", RegexOptions.CultureInvariant)]
    private static partial Regex SingleOffset();

    [GeneratedRegex(@"^(\d+)PP$", RegexOptions.CultureInvariant)]
    private static partial Regex SinglePrepaid();

    [GeneratedRegex(@"^(\d+)PP(\d+)-(\d+)D$", RegexOptions.CultureInvariant)]
    private static partial Regex PrepaidThenOffset();

    [GeneratedRegex(@"^(\d+)PP(\d+)PP$", RegexOptions.CultureInvariant)]
    private static partial Regex PrepaidThenPrepaid();

    /// <summary>Parses a code, throwing <see cref="FormatException"/> if it is unknown or the
    /// milestone percentages do not sum to 100 (±0.01). Use <see cref="TryParse"/> in the importer.</summary>
    public static PaymentTerms Parse(string raw)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(raw);
        var code = raw.Trim().ToUpperInvariant();

        var terms = TryBuild(code)
            ?? throw new FormatException($"Unrecognized payment-terms code '{raw}'.");

        var sum = terms.Milestones.Sum(m => m.Percentage);
        if (Math.Abs(sum - FullPercentage) > Tolerance)
        {
            throw new FormatException(
                $"Payment-terms '{code}' milestones sum to {sum}%, expected 100%.");
        }

        return terms;
    }

    /// <summary>Non-throwing parse for the importer: a bad code funnels to the rejection report
    /// instead of aborting the load.</summary>
    public static bool TryParse(string? raw, out PaymentTerms? terms)
    {
        terms = null;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        try
        {
            terms = Parse(raw);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static PaymentTerms? TryBuild(string code)
    {
        if (code == "DOMICILIADO")
        {
            // 100% on the invoice date (use FechaVencimiento when present; otherwise treat as 0D).
            return new PaymentTerms(
                code,
                [new PaymentMilestone(FullPercentage, PaymentTrigger.InvoiceDate, 0)],
                isDirectDebit: true);
        }

        if (PrepaidThenOffset().Match(code) is { Success: true } m3)
        {
            var a = ParsePct(m3.Groups[1].Value);
            var b = ParsePct(m3.Groups[2].Value);
            var days = int.Parse(m3.Groups[3].Value, CultureInfo.InvariantCulture);
            return new PaymentTerms(code,
            [
                new PaymentMilestone(a, PaymentTrigger.Prepaid, 0),
                new PaymentMilestone(b, PaymentTrigger.InvoiceDate, days),
            ], isDirectDebit: false);
        }

        if (PrepaidThenPrepaid().Match(code) is { Success: true } m4)
        {
            var a = ParsePct(m4.Groups[1].Value);
            var b = ParsePct(m4.Groups[2].Value);
            return new PaymentTerms(code,
            [
                new PaymentMilestone(a, PaymentTrigger.Prepaid, 0),
                new PaymentMilestone(b, PaymentTrigger.Prepaid, 0),
            ], isDirectDebit: false);
        }

        if (SinglePrepaid().Match(code) is { Success: true } m2)
        {
            var pct = ParsePct(m2.Groups[1].Value);
            return new PaymentTerms(code,
                [new PaymentMilestone(pct, PaymentTrigger.Prepaid, 0)], isDirectDebit: false);
        }

        if (SingleOffset().Match(code) is { Success: true } m1)
        {
            var days = int.Parse(m1.Groups[1].Value, CultureInfo.InvariantCulture);
            return new PaymentTerms(code,
                [new PaymentMilestone(FullPercentage, PaymentTrigger.InvoiceDate, days)], isDirectDebit: false);
        }

        return null;
    }

    private static decimal ParsePct(string raw) => decimal.Parse(raw, CultureInfo.InvariantCulture);

    /// <summary>
    /// Materialises the milestones for a given total and invoice date. Amounts are rounded to cents;
    /// the last milestone absorbs any rounding remainder so the parts always re-sum to
    /// <paramref name="total"/>. A prepaid milestone (or a missing invoice date) yields a null due date.
    /// </summary>
    public IEnumerable<(decimal Amount, DateTime? DueDate, decimal Percentage)> GenerateMilestones(
        decimal total, DateTime? invoiceDate)
    {
        decimal allocated = 0m;
        for (var i = 0; i < Milestones.Count; i++)
        {
            var milestone = Milestones[i];
            var isLast = i == Milestones.Count - 1;

            var amount = isLast
                ? total - allocated
                : Math.Round(total * milestone.Percentage / 100m, 2, MidpointRounding.AwayFromZero);
            allocated += amount;

            DateTime? dueDate = milestone.Trigger == PaymentTrigger.InvoiceDate && invoiceDate is { } date
                ? date.AddDays(milestone.OffsetDays)
                : null;

            yield return (amount, dueDate, milestone.Percentage);
        }
    }
}
