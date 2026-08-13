using FSH.Modules.Cashflow.Contracts.Enums;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Invoices;

/// <summary>One proposed invoice ↔ cash-line link, with a human-readable reason.</summary>
public sealed record LinkSuggestionDto(
    Guid InvoiceId,
    string? InvoiceNumber,
    InvoiceType InvoiceType,
    decimal InvoiceTotal,
    string? CounterpartyName,
    Guid LineId,
    CashLineKind Kind,
    decimal LineAmount,
    DateTime? LineDate,
    string? LineDescription,
    string? ProjectName,
    string Reason);

/// <summary>Result of the assisted-linking pass: only unambiguous, high-confidence pairs.
/// Ambiguous cases (several candidate lines for the same slot, or one line matching several
/// invoices) are counted but never suggested — those are resolved manually.</summary>
public sealed record LinkSuggestionsDto(
    IReadOnlyList<LinkSuggestionDto> Suggestions,
    int InvoicesWithoutLines,
    int UnlinkedLines,
    int AmbiguousInvoices);

/// <summary>Computes assisted-link suggestions between invoices that have no lines yet and
/// unlinked cash lines (§9 conciliación). Optional <paramref name="Type"/> narrows the side.</summary>
public sealed record GetLinkSuggestionsQuery(InvoiceType? Type = null) : IQuery<LinkSuggestionsDto>;
