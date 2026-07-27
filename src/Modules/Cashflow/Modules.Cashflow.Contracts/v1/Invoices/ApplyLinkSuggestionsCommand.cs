using FSH.Modules.Cashflow.Contracts.Enums;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Invoices;

/// <summary>One invoice ↔ line pair accepted by the user from the assisted pass.</summary>
public sealed record LinkPair(Guid InvoiceId, Guid LineId, CashLineKind Kind);

/// <summary>Outcome of a batch apply: how many pairs were linked and how many were skipped
/// (line already linked meanwhile, side mismatch, or missing rows) — skips never abort the batch.</summary>
public sealed record ApplyLinkSuggestionsResult(int Linked, int Skipped);

/// <summary>Applies a batch of accepted link suggestions. Each pair is re-validated server-side
/// (invoice exists, side matches, line still unlinked); invalid pairs are skipped, not fatal.</summary>
public sealed record ApplyLinkSuggestionsCommand(IReadOnlyList<LinkPair> Pairs)
    : ICommand<ApplyLinkSuggestionsResult>;
