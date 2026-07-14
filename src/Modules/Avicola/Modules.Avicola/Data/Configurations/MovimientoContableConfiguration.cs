using FSH.Modules.Avicola.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Avicola.Data.Configurations;

public sealed class MovimientoContableConfiguration : IEntityTypeConfiguration<MovimientoContable>
{
    public void Configure(EntityTypeBuilder<MovimientoContable> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Movimientos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Fecha).HasColumnName("Fecha").HasColumnType("timestamp with time zone");
        builder.Property(x => x.Tipo).HasColumnName("Tipo").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Categoria).HasColumnName("Categoria").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Concepto).HasColumnName("Concepto").IsRequired();
        builder.Property(x => x.Importe).HasColumnName("Importe").IsRequired();
        builder.Property(x => x.LoteId).HasColumnName("LoteId");
        builder.Property(x => x.Notas).HasColumnName("Notas");

        builder.HasOne<Lote>().WithMany().HasForeignKey(x => x.LoteId).OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(x => x.Tipo);
        builder.HasIndex(x => x.Categoria);

        builder.Ignore(x => x.DomainEvents);
    }
}
