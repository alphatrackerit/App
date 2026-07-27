using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Domain;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.GetLinkSuggestions;

/// <summary>
/// Pure matching engine for the assisted-link pass (§9). Precision over recall: it only proposes
/// pairs that are UNAMBIGUOUS — an invoice slot with several candidate lines, or a line wanted by
/// several invoices, produces no suggestion and is counted as ambiguous (resolved manually).
/// Counterparty semantics: for Payments it is the supplier; for Incomes the handler resolves it to
/// the line's project's client. Lines without a counterparty are never candidates.
/// </summary>
public static class LinkSuggestionMatcher
{
    private const decimal Tolerance = 0.01m;

    public sealed record InvoiceCandidate(
        Guid Id, string Number, InvoiceType Type, decimal Total,
        Guid? CounterpartyId, DateTime? InvoiceDate, PaymentTerms? Terms);

    public sealed record LineCandidate(
        Guid Id, decimal Amount, DateTime? Date, Guid? CounterpartyId, Guid? ProjectId, string? Description);

    public sealed record Match(InvoiceCandidate Invoice, LineCandidate Line, string Reason);

    public sealed record MatchResult(IReadOnlyList<Match> Matches, int AmbiguousInvoices);

    public static MatchResult Compute(
        IReadOnlyList<InvoiceCandidate> invoices, IReadOnlyList<LineCandidate> lines)
    {
        ArgumentNullException.ThrowIfNull(invoices);
        ArgumentNullException.ThrowIfNull(lines);

        var byCounterparty = lines
            .Where(l => l.CounterpartyId is not null)
            .GroupBy(l => l.CounterpartyId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var proposed = new List<Match>();
        var ambiguousInvoices = new HashSet<Guid>();

        foreach (var invoice in invoices)
        {
            if (invoice.CounterpartyId is null ||
                !byCounterparty.TryGetValue(invoice.CounterpartyId.Value, out var pool))
            {
                continue; // no counterparty to anchor on → manual
            }

            // 1) Whole-total single line — the common case (client: 1 factura = 1 hito; supplier
            //    single-payment schema). Exactly one candidate or nothing.
            var whole = pool.Where(l => Same(l.Amount, invoice.Total)).ToList();
            if (whole.Count == 1)
            {
                proposed.Add(new Match(invoice, whole[0], "IMPORTE EXACTO + CONTRAPARTE"));
                continue;
            }

            if (whole.Count > 1)
            {
                ambiguousInvoices.Add(invoice.Id);
                continue;
            }

            // 2) Milestone split (e.g. 30PP70-60D → 30% + 70%): every slot must resolve to exactly
            //    one distinct line, else the whole invoice is ambiguous/unmatched.
            if (invoice.Terms is null || invoice.Terms.Milestones.Count < 2)
            {
                continue;
            }

            var slots = invoice.Terms.GenerateMilestones(invoice.Total, invoice.InvoiceDate).ToList();
            var picked = new List<(LineCandidate Line, decimal Percentage)>();
            bool ambiguous = false, complete = true;

            foreach (var (amount, _, percentage) in slots)
            {
                var hits = pool
                    .Where(l => Same(l.Amount, amount) && picked.All(p => p.Line.Id != l.Id))
                    .ToList();
                if (hits.Count > 1)
                {
                    ambiguous = true;
                    break;
                }

                if (hits.Count == 0)
                {
                    complete = false;
                    break;
                }

                picked.Add((hits[0], percentage));
            }

            if (ambiguous)
            {
                ambiguousInvoices.Add(invoice.Id);
            }
            else if (complete)
            {
                foreach (var (line, pct) in picked)
                {
                    proposed.Add(new Match(invoice, line, $"HITO {pct:0.##}% ({invoice.Terms.Code})"));
                }
            }
        }

        // 3) Cross-invoice contention: a line proposed for two invoices is safe for neither.
        var contested = proposed
            .GroupBy(m => m.Line.Id)
            .Where(g => g.Select(m => m.Invoice.Id).Distinct().Count() > 1)
            .Select(g => g.Key)
            .ToHashSet();

        var contestedInvoices = proposed
            .Where(m => contested.Contains(m.Line.Id))
            .Select(m => m.Invoice.Id)
            .ToHashSet();

        var final = proposed.Where(m => !contestedInvoices.Contains(m.Invoice.Id)).ToList();
        ambiguousInvoices.UnionWith(contestedInvoices);

        return new MatchResult(final, ambiguousInvoices.Count);
    }

    private static bool Same(decimal a, decimal b) => Math.Abs(a - b) <= Tolerance;
}
