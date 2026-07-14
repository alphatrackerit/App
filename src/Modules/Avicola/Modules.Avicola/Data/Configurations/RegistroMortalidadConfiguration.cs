using FSH.Modules.Avicola.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Avicola.Data.Configurations;

public sealed class RegistroMortalidadConfiguration : IEntityTypeConfiguration<RegistroMortalidad>
{
    public void Configure(EntityTypeBuilder<RegistroMortalidad> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Mortalidad");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LoteId).HasColumnName("LoteId").IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("Fecha").HasColumnType("timestamp with time zone");
        builder.Property(x => x.Cantidad).HasColumnName("Cantidad").IsRequired();
        builder.Property(x => x.Descartes).HasColumnName("Descartes");
        builder.Property(x => x.Causa).HasColumnName("Causa");
        builder.Property(x => x.Notas).HasColumnName("Notas");

        // Owning flock — same module/schema → real FK (ON DELETE NO ACTION). EF adds the FK index.
        builder.HasOne<Lote>()
            .WithMany()
            .HasForeignKey(x => x.LoteId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Ignore(x => x.DomainEvents);
    }
}
