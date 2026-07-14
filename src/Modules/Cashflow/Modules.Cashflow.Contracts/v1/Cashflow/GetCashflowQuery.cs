using FSH.Modules.Cashflow.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Cashflow;

/// <summary>
/// Returns the tenant's full cash-flow ledger (every dated income and payment, all projects) plus
/// the project list. No pagination: the grid needs the complete series to compute the running
/// accumulated balance and to switch project/status filters client-side.
/// </summary>
public sealed record GetCashflowQuery : IQuery<CashflowDto>;
