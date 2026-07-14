using FSH.Modules.Avicola.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Avicola.Data.Configurations;

public sealed class PreparacionNaveConfiguration : IEntityTypeConfiguration<PreparacionNave>
{
    public void Configure(EntityTypeBuilder<PreparacionNave> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Preparaciones");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.GalponId).HasColumnName("GalponId").IsRequired();
        builder.Property(x => x.LoteAnteriorId).HasColumnName("LoteAnteriorId");
        builder.Property(x => x.FechaRetiro).HasColumnName("FechaRetiro").HasColumnType("timestamp with time zone");
        builder.Property(x => x.FechaInicio).HasColumnName("FechaInicio").HasColumnType("timestamp with time zone");
        builder.Property(x => x.FechaFin).HasColumnName("FechaFin").HasColumnType("timestamp with time zone");
        builder.Property(x => x.RetiradaCama).HasColumnName("RetiradaCama").IsRequired();
        builder.Property(x => x.Lavado).HasColumnName("Lavado").IsRequired();
        builder.Property(x => x.Desinfeccion).HasColumnName("Desinfeccion").IsRequired();
        builder.Property(x => x.Desinsectacion).HasColumnName("Desinsectacion").IsRequired();
        builder.Property(x => x.CamaNueva).HasColumnName("CamaNueva").IsRequired();
        builder.Property(x => x.Costo).HasColumnName("Costo");
        builder.Property(x => x.Estado).HasColumnName("Estado").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Notas).HasColumnName("Notas");

        builder.HasOne<Galpon>().WithMany().HasForeignKey(x => x.GalponId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<Lote>().WithMany().HasForeignKey(x => x.LoteAnteriorId).OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(x => x.Estado);

        builder.Ignore(x => x.DomainEvents);
    }
}
