using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Persistence.Context;
using FSH.Framework.Shared.Multitenancy;
using FSH.Framework.Shared.Persistence;
using FSH.Modules.Cashflow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace FSH.Modules.Cashflow.Data;

public sealed class CashflowDbContext : BaseDbContext
{
    public const string Schema = "cashflow";

    public CashflowDbContext(
        IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor,
        DbContextOptions<CashflowDbContext> options,
        IOptions<DatabaseOptions> settings,
        IHostEnvironment environment) : base(multiTenantContextAccessor, options, settings, environment) { }

    // Transactional
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Income> Incomes => Set<Income>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    // Catalog master-data (consolidated from the former Administration module)
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Society> Societies => Set<Society>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Status> Statuses => Set<Status>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Prefix> Prefixes => Set<Prefix>();

    // Bank statement import (spec §33, optional)
    public DbSet<Bank> Banks => Set<Bank>();
    public DbSet<BankMovement> BankMovements => Set<BankMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CashflowDbContext).Assembly);
        // base.OnModelCreating runs LAST so BaseDbContext's tenant-isolation auto-apply
        // sees fully-configured entities.
        base.OnModelCreating(modelBuilder);
    }
}
