using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Persistence.Context;
using FSH.Framework.Shared.Multitenancy;
using FSH.Framework.Shared.Persistence;
using FSH.Modules.Avicola.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace FSH.Modules.Avicola.Data;

public sealed class AvicolaDbContext : BaseDbContext
{
    public const string Schema = "avicola";

    public AvicolaDbContext(
        IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor,
        DbContextOptions<AvicolaDbContext> options,
        IOptions<DatabaseOptions> settings,
        IHostEnvironment environment) : base(multiTenantContextAccessor, options, settings, environment) { }

    public DbSet<Galpon> Galpones => Set<Galpon>();
    public DbSet<Lote> Lotes => Set<Lote>();
    public DbSet<RegistroMortalidad> Mortalidades => Set<RegistroMortalidad>();
    public DbSet<RegistroAlimentacion> Alimentaciones => Set<RegistroAlimentacion>();
    public DbSet<RegistroPeso> Pesos => Set<RegistroPeso>();
    public DbSet<RegistroSanitario> RegistrosSanitarios => Set<RegistroSanitario>();
    public DbSet<Despacho> Despachos => Set<Despacho>();
    public DbSet<Documento> Documentos => Set<Documento>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<MovimientoContable> Movimientos => Set<MovimientoContable>();
    public DbSet<PreparacionNave> Preparaciones => Set<PreparacionNave>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AvicolaDbContext).Assembly);
        // base.OnModelCreating runs LAST so BaseDbContext's tenant-isolation auto-apply
        // sees fully-configured entities.
        base.OnModelCreating(modelBuilder);
    }
}
