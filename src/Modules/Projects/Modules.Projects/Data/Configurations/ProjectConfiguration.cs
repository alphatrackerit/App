using FSH.Modules.Projects.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Projects.Data.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Proyectos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasColumnName("Nombre").IsRequired();
        builder.Property(x => x.SalePrice).HasColumnName("PrecioVenta");
        builder.Property(x => x.ForecastSale).HasColumnName("VentaPrevista");
        builder.Property(x => x.Cost).HasColumnName("Coste");
        builder.Property(x => x.ForecastCost).HasColumnName("CostePrevisto");
        builder.Property(x => x.Profit).HasColumnName("Beneficio");

        // External soft references (no FK constraint; indexed for lookups).
        builder.Property(x => x.ClientId).HasColumnName("ClienteId");
        builder.Property(x => x.SocietyId).HasColumnName("SociedadId");
        builder.Property(x => x.CountryId).HasColumnName("PaisId");
        builder.Property(x => x.CompanyId).HasColumnName("EmpresaId");
        builder.Property(x => x.StatusId).HasColumnName("EstadoId");
        builder.Property(x => x.PrefixId).HasColumnName("PrefijoId");

        builder.HasIndex(x => x.ClientId);
        builder.HasIndex(x => x.SocietyId);
        builder.HasIndex(x => x.CountryId);
        builder.HasIndex(x => x.CompanyId);
        builder.HasIndex(x => x.StatusId);
        builder.HasIndex(x => x.PrefixId);

        builder.Ignore(x => x.DomainEvents);
    }
}
