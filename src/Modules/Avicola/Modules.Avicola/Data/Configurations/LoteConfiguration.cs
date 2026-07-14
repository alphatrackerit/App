using FSH.Modules.Avicola.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Avicola.Data.Configurations;

public sealed class LoteConfiguration : IEntityTypeConfiguration<Lote>
{
    public void Configure(EntityTypeBuilder<Lote> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Lotes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Codigo).HasColumnName("Codigo").IsRequired();
        builder.Property(x => x.GalponId).HasColumnName("GalponId");
        builder.Property(x => x.Raza).HasColumnName("Raza");
        builder.Property(x => x.FechaIngreso).HasColumnName("FechaIngreso").HasColumnType("timestamp with time zone");
        builder.Property(x => x.CantidadInicial).HasColumnName("CantidadInicial");
        builder.Property(x => x.PesoInicialGramos).HasColumnName("PesoInicialGramos");
        builder.Property(x => x.FechaSalidaPrevista).HasColumnName("FechaSalidaPrevista").HasColumnType("timestamp with time zone");
        builder.Property(x => x.FechaSalidaReal).HasColumnName("FechaSalidaReal").HasColumnType("timestamp with time zone");
        builder.Property(x => x.Estado).HasColumnName("Estado").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.ProveedorId).HasColumnName("ProveedorId");
        builder.Property(x => x.CostoPolluelo).HasColumnName("CostoPolluelo");
        builder.Property(x => x.Notas).HasColumnName("Notas");

        // Owning shed — same module/schema → real FK (ON DELETE NO ACTION). EF adds the FK index.
        builder.HasOne<Galpon>()
            .WithMany()
            .HasForeignKey(x => x.GalponId)
            .OnDelete(DeleteBehavior.NoAction);

        // External soft reference (Proveedores) — indexed, no FK.
        builder.HasIndex(x => x.ProveedorId);
        builder.HasIndex(x => x.Estado);

        builder.Ignore(x => x.DomainEvents);
    }
}
