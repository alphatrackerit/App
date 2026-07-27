using FSH.Modules.Cashflow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Cashflow.Data.Configurations;

public sealed class BankMovementConfiguration : IEntityTypeConfiguration<BankMovement>
{
    public void Configure(EntityTypeBuilder<BankMovement> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("MovimientosBancos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Date).HasColumnName("Fecha").HasColumnType("timestamp without time zone");
        builder.Property(x => x.Concept).HasColumnName("Concepto").IsRequired();
        builder.Property(x => x.Amount).HasColumnName("Importe").IsRequired();
        builder.Property(x => x.Balance).HasColumnName("Saldo").IsRequired();
        builder.Property(x => x.BankName).HasColumnName("Banco").IsRequired();

        builder.HasIndex(x => x.BankName);
        builder.Ignore(x => x.DomainEvents);
    }
}
