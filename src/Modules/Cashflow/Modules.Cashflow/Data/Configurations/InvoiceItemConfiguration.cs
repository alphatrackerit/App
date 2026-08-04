using FSH.Modules.Cashflow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Cashflow.Data.Configurations;

public sealed class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("FacturaConceptos");
        builder.HasKey(x => x.Id);

        // Nav-collection child: without ValueGeneratedNever EF marks re-added rows Modified
        // instead of Added and the insert silently misbehaves (database.md).
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.InvoiceId).HasColumnName("FacturaId").IsRequired();
        builder.Property(x => x.Position).HasColumnName("Orden").IsRequired();
        builder.Property(x => x.Description).HasColumnName("Descripcion").HasMaxLength(512).IsRequired();
        builder.Property(x => x.Quantity).HasColumnName("Cantidad").IsRequired();
        builder.Property(x => x.UnitPrice).HasColumnName("PrecioUnitario").IsRequired();
        builder.Property(x => x.Amount).HasColumnName("Importe").IsRequired();

        builder.HasIndex(x => x.InvoiceId);
        builder.Ignore(x => x.DomainEvents);
    }
}
