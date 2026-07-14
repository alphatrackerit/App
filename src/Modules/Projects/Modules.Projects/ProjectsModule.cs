using Asp.Versioning;
using FSH.Framework.Persistence;
using FSH.Framework.Shared.Constants;
using FSH.Framework.Web.Modules;
using FSH.Modules.Projects.Data;
using FSH.Modules.Projects.Features.v1.Cashflow.GetCashflow;
using FSH.Modules.Projects.Features.v1.Incomes.ConfirmIncome;
using FSH.Modules.Projects.Features.v1.Incomes.CreateIncome;
using FSH.Modules.Projects.Features.v1.Incomes.DeleteIncome;
using FSH.Modules.Projects.Features.v1.Incomes.GetIncomeById;
using FSH.Modules.Projects.Features.v1.Incomes.SearchIncomes;
using FSH.Modules.Projects.Features.v1.Incomes.UpdateIncome;
using FSH.Modules.Projects.Features.v1.Incomes.ValidateIncome;
using FSH.Modules.Projects.Features.v1.Notes.CreateNote;
using FSH.Modules.Projects.Features.v1.Notes.DeleteNote;
using FSH.Modules.Projects.Features.v1.Notes.GetNoteById;
using FSH.Modules.Projects.Features.v1.Notes.SearchNotes;
using FSH.Modules.Projects.Features.v1.Notes.UpdateNote;
using FSH.Modules.Projects.Features.v1.Payments.ConfirmPayment;
using FSH.Modules.Projects.Features.v1.Payments.CreatePayment;
using FSH.Modules.Projects.Features.v1.Payments.DeletePayment;
using FSH.Modules.Projects.Features.v1.Payments.GetPaymentById;
using FSH.Modules.Projects.Features.v1.Payments.SearchPayments;
using FSH.Modules.Projects.Features.v1.Payments.UpdatePayment;
using FSH.Modules.Projects.Features.v1.Payments.ValidatePayment;
using FSH.Modules.Projects.Features.v1.Projects.CreateProject;
using FSH.Modules.Projects.Features.v1.Projects.DeleteProject;
using FSH.Modules.Projects.Features.v1.Projects.GetProjectById;
using FSH.Modules.Projects.Features.v1.Projects.SearchProjects;
using FSH.Modules.Projects.Features.v1.Projects.UpdateProject;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

[assembly: FshModule(typeof(FSH.Modules.Projects.ProjectsModule), 700)]

namespace FSH.Modules.Projects;

public sealed class ProjectsModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        FSH.Framework.Shared.Constants.PermissionConstants.Register(
            FSH.Modules.Projects.Contracts.Authorization.ProjectsPermissions.All);

        builder.Services.AddHeroDbContext<ProjectsDbContext>();
        builder.Services.AddScoped<IDbInitializer, ProjectsDbInitializer>();

        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ProjectsDbContext>(
                name: "db:projects",
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
            .WithTags("Projects")
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
    }
}
