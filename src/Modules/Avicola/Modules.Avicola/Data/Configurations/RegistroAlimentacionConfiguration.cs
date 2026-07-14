using FSH.Modules.Avicola.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Avicola.Data.Configurations;

public sealed class RegistroAlimentacionConfiguration : IEntityTypeConfiguration<RegistroAlimentacion>
{
    public void Configure(EntityTypeBuilder<RegistroAlimentacion> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Alimentacion");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LoteId).HasColumnName("LoteId").IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("Fecha").HasColumnType("timestamp with time zone");
        builder.Property(x => x.TipoAlimento).HasColumnName("TipoAlimento").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.CantidadKg).HasColumnName("CantidadKg").IsRequired();
        builder.Property(x => x.CostoUnitario).HasColumnName("CostoUnitario");
        builder.Property(x => x.Notas).HasColumnName("Notas");

        // Owning flock — same module/schema → real FK (ON DELETE NO ACTION). EF adds the FK index.
        builder.HasOne<Lote>()
            .WithMany()
            .HasForeignKey(x => x.LoteId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Ignore(x => x.DomainEvents);
    }
}
