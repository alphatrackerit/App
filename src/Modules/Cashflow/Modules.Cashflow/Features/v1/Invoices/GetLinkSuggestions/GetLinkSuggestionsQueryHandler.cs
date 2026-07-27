using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.GetLinkSuggestions;

public sealed class GetLinkSuggestionsQueryHandler(CashflowDbContext dbContext)
    : IQueryHandler<GetLinkSuggestionsQuery, LinkSuggestionsDto>
{
    public async ValueTask<LinkSuggestionsDto> Handle(GetLinkSuggestionsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Invoices that have no lines yet (clean state — partial links are resolved manually).
        var invoicesQ = dbContext.Invoices.AsNoTracking()
            .Where(i => !dbContext.Incomes.Any(l => l.InvoiceId == i.Id)
                     && !dbContext.Payments.Any(l => l.InvoiceId == i.Id));
        if (query.Type.HasValue)
        {
            invoicesQ = invoicesQ.Where(i => i.Type == query.Type.Value);
        }

        // Materialised (PaymentTerms is a converter — not projectable).
        var invoices = await invoicesQ.ToListAsync(cancellationToken).ConfigureAwait(false);

        // Project → client map resolves the Income side's counterparty.
        var projectClients = await dbContext.Projects.AsNoTracking()
            .Select(p => new { p.Id, p.ClientId, p.Name })
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var clientOfProject = projectClients.ToDictionary(p => p.Id, p => p.ClientId);
        var projectNames = projectClients.ToDictionary(p => p.Id, p => p.Name);

        var incomeLines = new List<LinkSuggestionMatcher.LineCandidate>();
        if (query.Type is null or InvoiceType.Emitida)
        {
            incomeLines = (await dbContext.Incomes.AsNoTracking()
                    .Where(l => l.InvoiceId == null)
                    .Select(l => new { l.Id, l.Amount, l.Date, l.ProjectId, l.Description })
                    .ToListAsync(cancellationToken).ConfigureAwait(false))
                .Select(l => new LinkSuggestionMatcher.LineCandidate(
                    l.Id, l.Amount, l.Date,
                    l.ProjectId is { } pid && clientOfProject.TryGetValue(pid, out var cid) ? cid : null,
                    l.ProjectId, l.Description))
                .ToList();
        }

        var paymentLines = new List<LinkSuggestionMatcher.LineCandidate>();
        if (query.Type is null or InvoiceType.Recibida)
        {
            paymentLines = (await dbContext.Payments.AsNoTracking()
                    .Where(l => l.InvoiceId == null)
                    .Select(l => new { l.Id, l.Amount, l.Date, l.SupplierId, l.ProjectId, l.Description })
                    .ToListAsync(cancellationToken).ConfigureAwait(false))
                .Select(l => new LinkSuggestionMatcher.LineCandidate(
                    l.Id, l.Amount, l.Date, l.SupplierId, l.ProjectId, l.Description))
                .ToList();
        }

        LinkSuggestionMatcher.InvoiceCandidate ToCandidate(Domain.Invoice i) => new(
            i.Id, i.Number, i.Type, i.Total,
            i.Type == InvoiceType.Emitida ? i.ClientId : i.SupplierId,
            i.InvoiceDate, i.PaymentTerms);

        var emitida = LinkSuggestionMatcher.Compute(
            invoices.Where(i => i.Type == InvoiceType.Emitida).Select(ToCandidate).ToList(), incomeLines);
        var recibida = LinkSuggestionMatcher.Compute(
            invoices.Where(i => i.Type == InvoiceType.Recibida).Select(ToCandidate).ToList(), paymentLines);

        // Display names for the counterparties involved.
        var clientNames = await dbContext.Clients.AsNoTracking()
            .Select(c => new { c.Id, c.Name }).ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken)
            .ConfigureAwait(false);
        var supplierNames = await dbContext.Suppliers.AsNoTracking()
            .Select(s => new { s.Id, s.Name }).ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken)
            .ConfigureAwait(false);

        LinkSuggestionDto ToDto(LinkSuggestionMatcher.Match m, CashLineKind kind)
        {
            string? counterparty = (m.Invoice.CounterpartyId, kind) switch
            {
                (null, _) => null,
                ({ } cp, CashLineKind.Income) => clientNames.GetValueOrDefault(cp),
                ({ } cp, _) => supplierNames.GetValueOrDefault(cp),
            };
            string? project = m.Line.ProjectId is { } pid ? projectNames.GetValueOrDefault(pid) : null;
            return new LinkSuggestionDto(
                m.Invoice.Id, m.Invoice.Number, m.Invoice.Type, m.Invoice.Total, counterparty,
                m.Line.Id, kind, m.Line.Amount, m.Line.Date, m.Line.Description, project, m.Reason);
        }

        var suggestions = emitida.Matches.Select(m => ToDto(m, CashLineKind.Income))
            .Concat(recibida.Matches.Select(m => ToDto(m, CashLineKind.Payment)))
            .OrderBy(s => s.InvoiceNumber, StringComparer.Ordinal)
            .ToList();

        return new LinkSuggestionsDto(
            suggestions,
            InvoicesWithoutLines: invoices.Count,
            UnlinkedLines: incomeLines.Count + paymentLines.Count,
            AmbiguousInvoices: emitida.AmbiguousInvoices + recibida.AmbiguousInvoices);
    }
}
