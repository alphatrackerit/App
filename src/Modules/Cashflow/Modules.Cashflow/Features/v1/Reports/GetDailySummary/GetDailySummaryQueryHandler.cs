using FSH.Modules.Cashflow.Contracts.Dtos;
using FSH.Modules.Cashflow.Contracts.v1.Reports;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Reports.GetDailySummary;

/// <summary>Server-side daily cash-flow summary — filters in the DB, aggregates per project/day in memory (spec §6).</summary>
public sealed class GetDailySummaryQueryHandler(CashflowDbContext db)
    : IQueryHandler<GetDailySummaryQuery, IReadOnlyList<DailySummaryProjectDto>>
{
    private const string TransparentColor = "#ffffff00";

    public async ValueTask<IReadOnlyList<DailySummaryProjectDto>> Handle(GetDailySummaryQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // ── Resolve the project-level filters (empresa / proyecto / estado del proyecto) to a project-id set. ──
        // Always applied: projects of a company hidden from Proyectos (ShowInProjects = false)
        // never operate in the cash flow. Projects without a company are unaffected.
        var pq = db.Projects.AsNoTracking()
            .Where(p => p.CompanyId == null || db.Companies.Any(c => c.Id == p.CompanyId && c.ShowInProjects));
        if (query.ProjectIds is { Count: > 0 })
        {
            pq = pq.Where(p => query.ProjectIds.Contains(p.Id));
        }
        if (query.CompanyIds is { Count: > 0 })
        {
            pq = pq.Where(p => p.CompanyId != null && query.CompanyIds.Contains(p.CompanyId.Value));
        }
        if (query.StatusIds is { Count: > 0 })
        {
            pq = pq.Where(p => p.StatusId != null && query.StatusIds.Contains(p.StatusId.Value));
        }
        List<Guid> allowedProjectIds = await pq.Select(p => p.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        if (allowedProjectIds.Count == 0)
        {
            return [];
        }

        DateTime? fromInclusive = query.From?.Date;
        DateTime? toExclusive = query.To?.Date.AddDays(1);

        // ── Load the filtered movements (dated only). ──
        var incQ = db.Incomes.AsNoTracking().Where(i => i.Date != null);
        var payQ = db.Payments.AsNoTracking().Where(p => p.Date != null);
        if (fromInclusive is not null)
        {
            incQ = incQ.Where(i => i.Date >= fromInclusive);
            payQ = payQ.Where(p => p.Date >= fromInclusive);
        }
        if (toExclusive is not null)
        {
            incQ = incQ.Where(i => i.Date < toExclusive);
            payQ = payQ.Where(p => p.Date < toExclusive);
        }
        if (query.OnlyConfirmed)
        {
            incQ = incQ.Where(i => i.Confirmed);
            payQ = payQ.Where(p => p.Confirmed);
        }
        if (query.OnlyValidated)
        {
            incQ = incQ.Where(i => i.Validated);
            payQ = payQ.Where(p => p.Validated);
        }
        incQ = incQ.Where(i => i.ProjectId != null && allowedProjectIds.Contains(i.ProjectId.Value));
        payQ = payQ.Where(p => p.ProjectId != null && allowedProjectIds.Contains(p.ProjectId.Value));

        var incomes = await incQ.ToListAsync(cancellationToken).ConfigureAwait(false);
        var payments = await payQ.ToListAsync(cancellationToken).ConfigureAwait(false);

        var projectIds = incomes.Where(i => i.ProjectId != null).Select(i => i.ProjectId!.Value)
            .Concat(payments.Where(p => p.ProjectId != null).Select(p => p.ProjectId!.Value))
            .Distinct().ToList();
        if (projectIds.Count == 0)
        {
            return [];
        }

        // ── Load the projects that have movements + all catalog names they (and the movements) reference. ──
        var projects = await db.Projects.AsNoTracking()
            .Where(p => projectIds.Contains(p.Id))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var statusIds = projects.Where(p => p.StatusId != null).Select(p => p.StatusId!.Value)
            .Concat(incomes.Where(i => i.StatusId != null).Select(i => i.StatusId!.Value))
            .Concat(payments.Where(p => p.StatusId != null).Select(p => p.StatusId!.Value))
            .Distinct().ToList();
        var statusNames = await db.Statuses.AsNoTracking()
            .Where(s => statusIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken).ConfigureAwait(false);

        var clientIds = projects.Where(p => p.ClientId != null).Select(p => p.ClientId!.Value).Distinct().ToList();
        var clientNames = await db.Clients.AsNoTracking().Where(c => clientIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken).ConfigureAwait(false);
        var companyIds = projects.Where(p => p.CompanyId != null).Select(p => p.CompanyId!.Value).Distinct().ToList();
        var companyNames = await db.Companies.AsNoTracking().Where(c => companyIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken).ConfigureAwait(false);
        var countryIds = projects.Where(p => p.CountryId != null).Select(p => p.CountryId!.Value).Distinct().ToList();
        var countryNames = await db.Countries.AsNoTracking().Where(c => countryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken).ConfigureAwait(false);

        var supplierIds = payments.Where(p => p.SupplierId != null).Select(p => p.SupplierId!.Value).Distinct().ToList();
        var suppliers = (await db.Suppliers.AsNoTracking()
            .Where(s => supplierIds.Contains(s.Id))
            .ToListAsync(cancellationToken).ConfigureAwait(false))
            .ToDictionary(s => s.Id);

        // Group the movements by project once (O(M)) instead of re-scanning per project.
        var incomesByProject = incomes.ToLookup(i => i.ProjectId);
        var paymentsByProject = payments.ToLookup(p => p.ProjectId);

        // ── Aggregate per project → per day. ──
        var result = new List<DailySummaryProjectDto>();
        foreach (var project in projects.OrderBy(p => p.Name))
        {
            var incByDay = incomesByProject[project.Id]
                .GroupBy(i => i.Date!.Value).ToDictionary(g => g.Key, g => g.ToList());
            var payByDay = paymentsByProject[project.Id]
                .GroupBy(p => p.Date!.Value).ToDictionary(g => g.Key, g => g.ToList());

            var dates = incByDay.Keys.Union(payByDay.Keys).OrderBy(d => d).ToList();

            decimal accumulated = 0m;
            var days = new List<DailySummaryDayDto>(dates.Count);
            foreach (var date in dates)
            {
                var dayIncomes = incByDay.TryGetValue(date, out var il) ? il : [];
                var dayPayments = payByDay.TryGetValue(date, out var pl) ? pl : [];

                decimal totalIncomes = dayIncomes.Sum(i => i.Amount);
                decimal totalPayments = dayPayments.Sum(p => p.Amount);
                accumulated += totalIncomes - totalPayments;

                // Predominant supplier of the day = highest VisualPriority among paid suppliers.
                string colorHex = TransparentColor;
                int visualPriority = int.MaxValue;
                var predominant = dayPayments
                    .Where(p => p.SupplierId != null && suppliers.ContainsKey(p.SupplierId.Value))
                    .Select(p => suppliers[p.SupplierId!.Value])
                    .OrderByDescending(s => s.VisualPriority)
                    .FirstOrDefault();
                if (predominant is not null)
                {
                    colorHex = predominant.ColorHex ?? TransparentColor;
                    visualPriority = predominant.VisualPriority;
                }

                var incomeDetails = dayIncomes.Select(i => new IncomeDetailDto(
                    i.Id, i.Amount, i.Percentage ?? 0m, Name(statusNames, i.StatusId),
                    i.Description, i.Confirmed, i.Validated)).ToList();
                var paymentDetails = dayPayments.Select(p => new PaymentDetailDto(
                    p.Id, p.Amount, p.Percentage ?? 0m, Name(statusNames, p.StatusId),
                    p.SupplierId != null && suppliers.TryGetValue(p.SupplierId.Value, out var sup) ? sup.Name : string.Empty,
                    p.Description, p.Confirmed, p.Validated)).ToList();

                days.Add(new DailySummaryDayDto(
                    date, totalIncomes, totalPayments, totalIncomes - totalPayments,
                    accumulated, colorHex, visualPriority, incomeDetails, paymentDetails));
            }

            result.Add(new DailySummaryProjectDto(
                project.Id,
                project.Name,
                Name(clientNames, project.ClientId),
                Name(companyNames, project.CompanyId),
                Name(countryNames, project.CountryId),
                Name(statusNames, project.StatusId),
                project.StatusId,
                days));
        }

        return result;
    }

    private static string Name(Dictionary<Guid, string> map, Guid? id) =>
        id != null && map.TryGetValue(id.Value, out var name) ? name : string.Empty;
}
