using FSH.Modules.Avicola.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Avicola.Data.Configurations;

public sealed class RegistroSanitarioConfiguration : IEntityTypeConfiguration<RegistroSanitario>
{
    public void Configure(EntityTypeBuilder<RegistroSanitario> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Sanidad");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LoteId).HasColumnName("LoteId").IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("Fecha").HasColumnType("timestamp with time zone");
        builder.Property(x => x.Tipo).HasColumnName("Tipo").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Producto).HasColumnName("Producto").IsRequired();
        builder.Property(x => x.Dosis).HasColumnName("Dosis");
        builder.Property(x => x.ViaAplicacion).HasColumnName("ViaAplicacion");
        builder.Property(x => x.Costo).HasColumnName("Costo");
        builder.Property(x => x.Notas).HasColumnName("Notas");

        // Owning flock — same module/schema → real FK (ON DELETE NO ACTION). EF adds the FK index.
        builder.HasOne<Lote>()
            .WithMany()
            .HasForeignKey(x => x.LoteId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Ignore(x => x.DomainEvents);
    }
}
