using FSH.Modules.Cashflow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Cashflow.Data.Configurations;

public sealed class StatusConfiguration : IEntityTypeConfiguration<Status>
{
    public void Configure(EntityTypeBuilder<Status> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("Estados");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasColumnName("Nombre").IsRequired().HasMaxLength(256);
        builder.Property(x => x.Code).HasColumnName("Codigo").HasMaxLength(64);
        builder.Property(x => x.Type).HasColumnName("Tipo").HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.ColorHex).HasColumnName("ColorHex").HasMaxLength(32);
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.Type);
        builder.Ignore(x => x.DomainEvents);
    }
}
