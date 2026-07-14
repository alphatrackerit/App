using FSH.Modules.Cashflow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Cashflow.Data.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Pagos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).HasColumnName("Importe").IsRequired();
        builder.Property(x => x.Description).HasColumnName("Descripcion");
        builder.Property(x => x.Date).HasColumnName("Fecha").HasColumnType("timestamp with time zone");
        builder.Property(x => x.Percentage).HasColumnName("Porcentaje");
        builder.Property(x => x.SupplierId).HasColumnName("ProveedorId");
        builder.Property(x => x.ProjectId).HasColumnName("ProyectoId");
        builder.Property(x => x.StatusId).HasColumnName("EstadoId");
        builder.Property(x => x.Confirmed).HasColumnName("Confirmado").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.Validated).HasColumnName("Validado").IsRequired().HasDefaultValue(false);

        // Owning project — same module/schema → real FK (ON DELETE NO ACTION). EF adds the FK index.
        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.NoAction);

        // External soft references (Proveedores, Estados) — indexed, no FK.
        builder.HasIndex(x => x.SupplierId);
        builder.HasIndex(x => x.StatusId);

        builder.Ignore(x => x.DomainEvents);
    }
}
