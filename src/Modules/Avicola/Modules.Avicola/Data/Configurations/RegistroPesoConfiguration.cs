using FSH.Modules.Avicola.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Avicola.Data.Configurations;

public sealed class RegistroPesoConfiguration : IEntityTypeConfiguration<RegistroPeso>
{
    public void Configure(EntityTypeBuilder<RegistroPeso> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Pesos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LoteId).HasColumnName("LoteId").IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("Fecha").HasColumnType("timestamp with time zone");
        builder.Property(x => x.PesoPromedioGramos).HasColumnName("PesoPromedioGramos").IsRequired();
        builder.Property(x => x.CantidadMuestra).HasColumnName("CantidadMuestra");
        builder.Property(x => x.Notas).HasColumnName("Notas");

        // Owning flock — same module/schema → real FK (ON DELETE NO ACTION). EF adds the FK index.
        builder.HasOne<Lote>()
            .WithMany()
            .HasForeignKey(x => x.LoteId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Ignore(x => x.DomainEvents);
    }
}
