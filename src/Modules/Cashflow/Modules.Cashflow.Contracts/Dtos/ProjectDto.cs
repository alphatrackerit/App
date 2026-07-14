namespace FSH.Modules.Cashflow.Contracts.Dtos;

public sealed record ProjectDto(
    Guid Id,
    string Name,
    decimal? SalePrice,
    decimal? ForecastSale,
    decimal? Cost,
    decimal? ForecastCost,
    decimal? Profit,
    Guid? ClientId,
    Guid? SocietyId,
    Guid? CountryId,
    Guid? CompanyId,
    Guid? StatusId,
    Guid? PrefixId);
