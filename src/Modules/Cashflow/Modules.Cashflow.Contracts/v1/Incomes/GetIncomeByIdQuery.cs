using FSH.Modules.Cashflow.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Incomes;

public sealed record GetIncomeByIdQuery(Guid IncomeId) : IQuery<IncomeDto>;
