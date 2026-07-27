using FSH.Modules.Cashflow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Cashflow.Data.Configurations;

public sealed class BankConfiguration : IEntityTypeConfiguration<Bank>
{
    public void Configure(EntityTypeBuilder<Bank> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Bancos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasColumnName("Nombre").IsRequired().HasMaxLength(256);
        builder.Property(x => x.StartRow).HasColumnName("FilaInicio").IsRequired();
        builder.Property(x => x.DateColumn).HasColumnName("FechaColumna").IsRequired();
        builder.Property(x => x.ConceptColumn).HasColumnName("ConceptoColumna").IsRequired();
        builder.Property(x => x.AmountColumn).HasColumnName("ImporteColumna").IsRequired();
        builder.Property(x => x.BalanceColumn).HasColumnName("SaldoColumna").IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("IsActive").IsRequired().HasDefaultValue(true);
        builder.Property(x => x.Notes).HasColumnName("Notas");

        builder.HasIndex(x => x.Name);
        builder.Ignore(x => x.DomainEvents);
    }
}
