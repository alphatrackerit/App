using FSH.Modules.Cashflow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Cashflow.Data.Configurations;

public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("Paises");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasColumnName("Nombre").IsRequired().HasMaxLength(256);
        builder.Property(x => x.Code).HasColumnName("CodigoIso").HasMaxLength(64);
        builder.Property(x => x.Description).HasColumnName("Descripcion").HasMaxLength(1024);
        builder.HasIndex(x => x.Name);
        builder.Ignore(x => x.DomainEvents);
    }
}
