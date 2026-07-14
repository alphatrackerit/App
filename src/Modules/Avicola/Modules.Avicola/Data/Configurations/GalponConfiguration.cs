using FSH.Modules.Avicola.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Avicola.Data.Configurations;

public sealed class GalponConfiguration : IEntityTypeConfiguration<Galpon>
{
    public void Configure(EntityTypeBuilder<Galpon> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Galpones");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre).HasColumnName("Nombre").IsRequired();
        builder.Property(x => x.Codigo).HasColumnName("Codigo");
        builder.Property(x => x.Capacidad).HasColumnName("Capacidad");
        builder.Property(x => x.SuperficieM2).HasColumnName("SuperficieM2");
        builder.Property(x => x.Ubicacion).HasColumnName("Ubicacion");
        builder.Property(x => x.Activo).HasColumnName("Activo").IsRequired();
        builder.Property(x => x.Notas).HasColumnName("Notas");

        builder.Ignore(x => x.DomainEvents);
    }
}
