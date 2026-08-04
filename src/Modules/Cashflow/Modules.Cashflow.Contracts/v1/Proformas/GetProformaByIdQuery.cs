using FSH.Modules.Cashflow.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Proformas;

public sealed record GetProformaByIdQuery(Guid ProformaId) : IQuery<ProformaDto>;
