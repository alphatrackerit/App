using FSH.Framework.Core.Domain;

namespace FSH.Modules.Cashflow.Domain;

public sealed class Project : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public decimal? SalePrice { get; private set; }
    public decimal? ForecastSale { get; private set; }
    public decimal? Cost { get; private set; }
    public decimal? ForecastCost { get; private set; }
    public decimal? Profit { get; private set; }

    // External references (Clientes / Sociedades / Paises / Empresas / Estados / Prefijos).
    // Those catalogs live outside this bounded context, so these are soft references
    // (indexed Guid columns) — NOT database foreign keys (modular-monolith boundary).
    public Guid? ClientId { get; private set; }
    public Guid? SocietyId { get; private set; }
    public Guid? CountryId { get; private set; }
    public Guid? CompanyId { get; private set; }
    public Guid? StatusId { get; private set; }
    public Guid? PrefixId { get; private set; }

    private Project() { }

    public static Project Create(
        string name,
        decimal? salePrice,
        decimal? forecastSale,
        decimal? cost,
        decimal? forecastCost,
        decimal? profit,
        Guid? clientId,
        Guid? societyId,
        Guid? countryId,
        Guid? companyId,
        Guid? statusId,
        Guid? prefixId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Project
        {
            Id = Guid.CreateVersion7(),
            Name = name.Trim(),
            SalePrice = salePrice,
            ForecastSale = forecastSale,
            Cost = cost,
            ForecastCost = forecastCost,
            Profit = profit,
            ClientId = clientId,
            SocietyId = societyId,
            CountryId = countryId,
            CompanyId = companyId,
            StatusId = statusId,
            PrefixId = prefixId,
        };
    }

    public void Update(
        string name,
        decimal? salePrice,
        decimal? forecastSale,
        decimal? cost,
        decimal? forecastCost,
        decimal? profit,
        Guid? clientId,
        Guid? societyId,
        Guid? countryId,
        Guid? companyId,
        Guid? statusId,
        Guid? prefixId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        SalePrice = salePrice;
        ForecastSale = forecastSale;
        Cost = cost;
        ForecastCost = forecastCost;
        Profit = profit;
        ClientId = clientId;
        SocietyId = societyId;
        CountryId = countryId;
        CompanyId = companyId;
        StatusId = statusId;
        PrefixId = prefixId;
    }
}
