using FSH.Modules.Cashflow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Cashflow.Data.Configurations;

public sealed class IncomeConfiguration : IEntityTypeConfiguration<Income>
{
    public void Configure(EntityTypeBuilder<Income> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Ingresos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).HasColumnName("Importe").IsRequired();
        builder.Property(x => x.Description).HasColumnName("Descripcion");
        builder.Property(x => x.Date).HasColumnName("Fecha").HasColumnType("timestamp without time zone");
        builder.Property(x => x.Percentage).HasColumnName("Porcentaje");
        builder.Property(x => x.ProjectId).HasColumnName("ProyectoId");
        builder.Property(x => x.StatusId).HasColumnName("EstadoId");
        builder.Property(x => x.InvoiceId).HasColumnName("FacturaId");
        builder.Property(x => x.Confirmed).HasColumnName("Confirmado").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.Validated).HasColumnName("Validado").IsRequired().HasDefaultValue(false);

        // Owning project — same module/schema → real FK (ON DELETE NO ACTION). EF adds the FK index.
        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.NoAction);

        // Owning invoice — same module/schema → real FK. ON DELETE SET NULL: deleting an invoice
        // unlinks its lines back to forecast state without erasing their project impact (§3). EF adds the index.
        builder.HasOne<Invoice>()
            .WithMany()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.SetNull);

        // External soft reference (Estados) — indexed, no FK.
        builder.HasIndex(x => x.StatusId);

        builder.Ignore(x => x.DomainEvents);
    }
}
