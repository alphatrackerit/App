using Mediator;

namespace FSH.Modules.Projects.Contracts.v1.Projects;

public sealed record UpdateProjectCommand(
    Guid ProjectId,
    string Name,
    decimal? SalePrice = null,
    decimal? ForecastSale = null,
    decimal? Cost = null,
    decimal? ForecastCost = null,
    decimal? Profit = null,
    Guid? ClientId = null,
    Guid? SocietyId = null,
    Guid? CountryId = null,
    Guid? CompanyId = null,
    Guid? StatusId = null,
    Guid? PrefixId = null) : ICommand<Guid>;
