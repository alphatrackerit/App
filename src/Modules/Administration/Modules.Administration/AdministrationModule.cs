using Asp.Versioning;
using FSH.Framework.Persistence;
using FSH.Framework.Web.Modules;
using FSH.Modules.Administration.Data;
using FSH.Modules.Administration.Features.v1.Clients;
using FSH.Modules.Administration.Features.v1.Companies;
using FSH.Modules.Administration.Features.v1.Countries;
using FSH.Modules.Administration.Features.v1.Statuses;
using FSH.Modules.Administration.Features.v1.Suppliers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

[assembly: FshModule(typeof(FSH.Modules.Administration.AdministrationModule), 750)]

namespace FSH.Modules.Administration;

public sealed class AdministrationModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        FSH.Framework.Shared.Constants.PermissionConstants.Register(
            FSH.Modules.Administration.Contracts.Authorization.AdministrationPermissions.All);

        builder.Services.AddHeroDbContext<AdministrationDbContext>();
        builder.Services.AddScoped<IDbInitializer, AdministrationDbInitializer>();

        builder.Services.AddHealthChecks()
            .AddDbContextCheck<AdministrationDbContext>(
                name: "db:administration",
                failureStatus: HealthStatus.Unhealthy);
    }

    public void ConfigureMiddleware(IApplicationBuilder app)
    {
        // No custom middleware needed.
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = endpoints
            .MapGroup("api/v{version:apiVersion}")
            .WithTags("Administration")
            .WithApiVersionSet(versionSet)
            .RequireAuthorization();

        group.MapSearchClientsEndpoint();
        group.MapCreateClientEndpoint();
        group.MapUpdateClientEndpoint();
        group.MapDeleteClientEndpoint();

        group.MapSearchSuppliersEndpoint();
        group.MapCreateSupplierEndpoint();
        group.MapUpdateSupplierEndpoint();
        group.MapDeleteSupplierEndpoint();

        group.MapSearchCountriesEndpoint();
        group.MapCreateCountryEndpoint();
        group.MapUpdateCountryEndpoint();
        group.MapDeleteCountryEndpoint();

        group.MapSearchStatusesEndpoint();
        group.MapCreateStatusEndpoint();
        group.MapUpdateStatusEndpoint();
        group.MapDeleteStatusEndpoint();

        group.MapSearchCompaniesEndpoint();
        group.MapCreateCompanyEndpoint();
        group.MapUpdateCompanyEndpoint();
        group.MapDeleteCompanyEndpoint();
    }
}
