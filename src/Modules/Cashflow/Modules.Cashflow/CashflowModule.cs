using Asp.Versioning;
using FSH.Framework.Persistence;
using Hangfire;
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
using FSH.Modules.Cashflow.Features.v1.Invoices.CreateInvoice;
using FSH.Modules.Cashflow.Features.v1.Invoices.UpdateInvoice;
using FSH.Modules.Cashflow.Features.v1.Invoices.DeleteInvoice;
using FSH.Modules.Cashflow.Features.v1.Invoices.GetInvoiceById;
using FSH.Modules.Cashflow.Features.v1.Invoices.SearchInvoices;
using FSH.Modules.Cashflow.Features.v1.Invoices.LinkLineToInvoice;
using FSH.Modules.Cashflow.Features.v1.Invoices.GetLinkSuggestions;
using FSH.Modules.Cashflow.Features.v1.Invoices.ApplyLinkSuggestions;
using FSH.Modules.Cashflow.Features.v1.Invoices.GenerateMilestones;
using FSH.Modules.Cashflow.Features.v1.Invoices.MarkInvoiceVerified;
using FSH.Modules.Cashflow.Features.v1.Invoices.GetInvoiceLines;
using FSH.Modules.Cashflow.Features.v1.Invoices.ExtractInvoice;
using FSH.Modules.Cashflow.Features.v1.Invoices.AttachInvoiceDocument;
using FSH.Modules.Cashflow.Features.v1.Invoices.GetInvoiceDocument;
using FSH.Modules.Cashflow.Features.v1.Proformas.AttachProformaDocument;
using FSH.Modules.Cashflow.Features.v1.Proformas.CreateProforma;
using FSH.Modules.Cashflow.Features.v1.Proformas.DeleteProforma;
using FSH.Modules.Cashflow.Features.v1.Proformas.GenerateInvoicesFromProforma;
using FSH.Modules.Cashflow.Features.v1.Proformas.GetProformaById;
using FSH.Modules.Cashflow.Features.v1.Proformas.GetProformaDocument;
using FSH.Modules.Cashflow.Features.v1.Proformas.GetProformaLines;
using FSH.Modules.Cashflow.Features.v1.Proformas.GetProformaPdf;
using FSH.Modules.Cashflow.Features.v1.Proformas.LinkInvoiceToProforma;
using FSH.Modules.Cashflow.Features.v1.Proformas.SearchProformas;
using FSH.Modules.Cashflow.Features.v1.Proformas.UpdateProforma;
using FSH.Modules.Cashflow.Features.v1.Invoices.IssueInvoiceVerifactu;
using FSH.Modules.Cashflow.Features.v1.Invoices.GetInvoicePdf;
using FSH.Modules.Cashflow.Features.v1.Projects.CreateProject;
using FSH.Modules.Cashflow.Features.v1.VerifactuSettings;
using FSH.Modules.Cashflow.Services;
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
using FSH.Modules.Cashflow.Features.v1.Banks;
using FSH.Modules.Cashflow.Features.v1.BankMovements;
using FSH.Modules.Cashflow.Features.v1.Reports.GetDailySummary;
using FSH.Modules.Cashflow.Features.v1.Reports.GetInvoicesReport;
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

        builder.Services.AddSingleton<IProformaPdfRenderer, ProformaPdfRenderer>();

        // VERI*FACTU: pure chain logic, DataProtection-backed certificate storage, PDF w/ QR,
        // AEAT SOAP client (per-company mTLS) and the Hangfire submission jobs.
        builder.Services.AddSingleton<IVerifactuChainService, VerifactuChainService>();
        builder.Services.AddSingleton<IVerifactuSecretProtector, VerifactuSecretProtector>();
        builder.Services.AddSingleton<ICashflowInvoicePdfRenderer, CashflowInvoicePdfRenderer>();
        builder.Services.AddSingleton<IAeatVerifactuClient, AeatVerifactuClient>();
        builder.Services.AddScoped<Jobs.SubmitVerifactuRecordsJob>();
        builder.Services.AddScoped<Jobs.VerifactuSweepJob>();

        // AI-assisted invoice extraction (CashflowAi__ApiKey / ANTHROPIC_API_KEY at runtime).
        builder.Services.Configure<CashflowAiOptions>(
            builder.Configuration.GetSection(CashflowAiOptions.SectionName));

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

        // Invoices (facturación)
        group.MapSearchInvoicesEndpoint();
        group.MapCreateInvoiceEndpoint();
        group.MapGetInvoiceByIdEndpoint();
        group.MapGetInvoiceLinesEndpoint();
        group.MapUpdateInvoiceEndpoint();
        group.MapDeleteInvoiceEndpoint();
        group.MapLinkLineToInvoiceEndpoint();
        group.MapGetLinkSuggestionsEndpoint();
        group.MapApplyLinkSuggestionsEndpoint();
        group.MapGenerateMilestonesEndpoint();
        group.MapMarkInvoiceVerifiedEndpoint();
        group.MapExtractInvoiceEndpoint();
        group.MapAttachInvoiceDocumentEndpoint();
        group.MapGetInvoiceDocumentEndpoint();

        // Presentable invoice PDF (QR/leyenda AEAT automáticos si está registrada en VeriFactu)
        group.MapGetCashflowInvoicePdfEndpoint();

        // VERI*FACTU (AEAT)
        group.MapIssueInvoiceVerifactuEndpoint();
        group.MapGetVerifactuSettingsEndpoint();
        group.MapUpsertVerifactuSettingsEndpoint();
        group.MapSetVerifactuCertificateEndpoint();

        // Proformas (1 proforma → N invoices)
        group.MapSearchProformasEndpoint();
        group.MapCreateProformaEndpoint();
        group.MapGetProformaByIdEndpoint();
        group.MapGetProformaLinesEndpoint();
        group.MapUpdateProformaEndpoint();
        group.MapDeleteProformaEndpoint();
        group.MapGenerateInvoicesFromProformaEndpoint();
        group.MapLinkInvoiceToProformaEndpoint();
        group.MapAttachProformaDocumentEndpoint();
        group.MapGetProformaDocumentEndpoint();
        group.MapGetProformaPdfEndpoint();

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
        group.MapSetCompanyLogoEndpoint();
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

        // Banks (statement-import column mapping — spec §33)
        group.MapSearchBanksEndpoint();
        group.MapCreateBankEndpoint();
        group.MapUpdateBankEndpoint();
        group.MapDeleteBankEndpoint();

        // Bank movements
        group.MapSearchBankMovementsEndpoint();
        group.MapCreateBankMovementEndpoint();
        group.MapUpdateBankMovementEndpoint();
        group.MapDeleteBankMovementEndpoint();

        // Reports (server-side aggregation)
        group.MapGetDailySummaryEndpoint();
        group.MapGetInvoicesReportEndpoint();

        // VERI*FACTU hourly sweep — retries PendienteEnvio/ErrorTecnico records (covers transient
        // AEAT failures and "certificate just uploaded, backlog waiting").
        var jobManager = endpoints.ServiceProvider.GetService<IRecurringJobManager>();
        jobManager?.AddOrUpdate<Jobs.VerifactuSweepJob>(
            "cashflow:verifactu-sweep",
            j => j.RunAsync(CancellationToken.None),
            Cron.Hourly(),
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
    }
}
