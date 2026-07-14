using FSH.Modules.Avicola.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Avicola.Data.Configurations;

public sealed class DespachoConfiguration : IEntityTypeConfiguration<Despacho>
{
    public void Configure(EntityTypeBuilder<Despacho> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Despachos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LoteId).HasColumnName("LoteId").IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("Fecha").HasColumnType("timestamp with time zone");
        builder.Property(x => x.Cantidad).HasColumnName("Cantidad").IsRequired();
        builder.Property(x => x.PesoTotalKg).HasColumnName("PesoTotalKg").IsRequired();
        builder.Property(x => x.PrecioPorKg).HasColumnName("PrecioPorKg");
        builder.Property(x => x.ClienteId).HasColumnName("ClienteId");
        builder.Property(x => x.Notas).HasColumnName("Notas");

        // Owning flock — same module/schema → real FK (ON DELETE NO ACTION). EF adds the FK index.
        builder.HasOne<Lote>()
            .WithMany()
            .HasForeignKey(x => x.LoteId)
            .OnDelete(DeleteBehavior.NoAction);

        // External soft reference (Clientes) — indexed, no FK.
        builder.HasIndex(x => x.ClienteId);

        builder.Ignore(x => x.DomainEvents);
    }
}
