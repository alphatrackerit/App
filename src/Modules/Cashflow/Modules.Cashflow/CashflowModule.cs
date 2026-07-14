using Asp.Versioning;
using FSH.Framework.Persistence;
using FSH.Framework.Shared.Constants;
using FSH.Framework.Web.Modules;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Features.v1.Cashflow.GetCashflow;
using FSH.Modules.Cashflow.Features.v1.Incomes.ConfirmIncome;
using FSH.Modules.Cashflow.Features.v1.Incomes.CreateIncome;
using FSH.Modules.Cashflow.Features.v1.Incomes.DeleteIncome;
using FSH.Modules.Cashflow.Features.v1.Incomes.GetIncomeById;
using FSH.Modules.Cashflow.Features.v1.Incomes.SearchIncomes;
using FSH.Modules.Cashflow.Features.v1.Incomes.UpdateIncome;
using FSH.Modules.Cashflow.Features.v1.Incomes.ValidateIncome;
using FSH.Modules.Cashflow.Features.v1.Notes.CreateNote;
using FSH.Modules.Cashflow.Features.v1.Notes.DeleteNote;
using FSH.Modules.Cashflow.Features.v1.Notes.GetNoteById;
using FSH.Modules.Cashflow.Features.v1.Notes.SearchNotes;
using FSH.Modules.Cashflow.Features.v1.Notes.UpdateNote;
using FSH.Modules.Cashflow.Features.v1.Payments.ConfirmPayment;
using FSH.Modules.Cashflow.Features.v1.Payments.CreatePayment;
using FSH.Modules.Cashflow.Features.v1.Payments.DeletePayment;
using FSH.Modules.Cashflow.Features.v1.Payments.GetPaymentById;
using FSH.Modules.Cashflow.Features.v1.Payments.SearchPayments;
using FSH.Modules.Cashflow.Features.v1.Payments.UpdatePayment;
using FSH.Modules.Cashflow.Features.v1.Payments.ValidatePayment;
using FSH.Modules.Cashflow.Features.v1.Projects.CreateProject;
using FSH.Modules.Cashflow.Features.v1.Projects.DeleteProject;
using FSH.Modules.Cashflow.Features.v1.Projects.GetProjectById;
using FSH.Modules.Cashflow.Features.v1.Projects.SearchProjects;
using FSH.Modules.Cashflow.Features.v1.Projects.UpdateProject;
using FSH.Modules.Cashflow.Features.v1.Clients;
using FSH.Modules.Cashflow.Features.v1.Suppliers;
using FSH.Modules.Cashflow.Features.v1.Countries;
using FSH.Modules.Cashflow.Features.v1.Statuses;
using FSH.Modules.Cashflow.Features.v1.Companies;
using FSH.Modules.Cashflow.Features.v1.Societies;
using FSH.Modules.Cashflow.Features.v1.Prefixes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

[assembly: FshModule(typeof(FSH.Modules.Cashflow.CashflowModule), 700)]

namespace FSH.Modules.Cashflow;

public sealed class CashflowModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        FSH.Framework.Shared.Constants.PermissionConstants.Register(
            FSH.Modules.Cashflow.Contracts.Authorization.CashflowPermissions.All);

        builder.Services.AddHeroDbContext<CashflowDbContext>();
        builder.Services.AddScoped<IDbInitializer, CashflowDbInitializer>();

        builder.Services.AddHealthChecks()
            .AddDbContextCheck<CashflowDbContext>(
                name: "db:cashflow",
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
            .MapGroup("api/v{version:apiVersion}/cashflow")
            .WithTags("Cashflow")
            .WithApiVersionSet(versionSet)
            // Named policy so .RequirePermission() gates run; a bare RequireAuthorization()
            // swaps in the authenticated-only default policy and skips permission checks.
            .RequireAuthorization(PermissionConstants.RequiredPermissionPolicyName);

        // Projects
        group.MapSearchProjectsEndpoint();
        group.MapCreateProjectEndpoint();
        group.MapGetProjectByIdEndpoint();
        group.MapUpdateProjectEndpoint();
        group.MapDeleteProjectEndpoint();

        // Incomes
        group.MapSearchIncomesEndpoint();
        group.MapCreateIncomeEndpoint();
        group.MapGetIncomeByIdEndpoint();
        group.MapUpdateIncomeEndpoint();
        group.MapDeleteIncomeEndpoint();
        group.MapConfirmIncomeEndpoint();
        group.MapValidateIncomeEndpoint();

        // Payments
        group.MapSearchPaymentsEndpoint();
        group.MapCreatePaymentEndpoint();
        group.MapGetPaymentByIdEndpoint();
        group.MapUpdatePaymentEndpoint();
        group.MapDeletePaymentEndpoint();
        group.MapConfirmPaymentEndpoint();
        group.MapValidatePaymentEndpoint();

        // Notes
        group.MapSearchNotesEndpoint();
        group.MapCreateNoteEndpoint();
        group.MapGetNoteByIdEndpoint();
        group.MapUpdateNoteEndpoint();
        group.MapDeleteNoteEndpoint();

        // Cash flow (aggregate)
        group.MapGetCashflowEndpoint();

        // ── Catalog master-data (consolidated from the former Administration module) ──
        // Clients
        group.MapSearchClientsEndpoint();
        group.MapCreateClientEndpoint();
        group.MapUpdateClientEndpoint();
        group.MapDeleteClientEndpoint();

        // Suppliers
        group.MapSearchSuppliersEndpoint();
        group.MapCreateSupplierEndpoint();
        group.MapUpdateSupplierEndpoint();
        group.MapDeleteSupplierEndpoint();

        // Countries
        group.MapSearchCountriesEndpoint();
        group.MapCreateCountryEndpoint();
        group.MapUpdateCountryEndpoint();
        group.MapDeleteCountryEndpoint();

        // Statuses
        group.MapSearchStatusesEndpoint();
        group.MapCreateStatusEndpoint();
        group.MapUpdateStatusEndpoint();
        group.MapDeleteStatusEndpoint();

        // Companies
        group.MapSearchCompaniesEndpoint();
        group.MapCreateCompanyEndpoint();
        group.MapUpdateCompanyEndpoint();
        group.MapDeleteCompanyEndpoint();

        // Societies
        group.MapSearchSocietiesEndpoint();
        group.MapCreateSocietyEndpoint();
        group.MapUpdateSocietyEndpoint();
        group.MapDeleteSocietyEndpoint();

        // Prefixes
        group.MapSearchPrefixesEndpoint();
        group.MapCreatePrefixEndpoint();
        group.MapUpdatePrefixEndpoint();
        group.MapDeletePrefixEndpoint();
    }
}
