using FSH.Modules.Cashflow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Cashflow.Data.Configurations;

public sealed class SocietyConfiguration : IEntityTypeConfiguration<Society>
{
    public void Configure(EntityTypeBuilder<Society> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("Sociedades");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasColumnName("Nombre").IsRequired().HasMaxLength(256);
        builder.Property(x => x.TaxId).HasColumnName("NifCif").HasMaxLength(64);
        builder.Property(x => x.Address).HasColumnName("Direccion").HasMaxLength(512);
        builder.Property(x => x.PostalCode).HasColumnName("CodigoPostal").HasMaxLength(32);
        builder.Property(x => x.City).HasColumnName("Ciudad").HasMaxLength(128);
        builder.Property(x => x.Country).HasColumnName("Pais").HasMaxLength(128);
        builder.Property(x => x.ClientId).HasColumnName("ClienteId");

        // Owning client — same module/schema → real FK (ON DELETE NO ACTION). EF adds the FK index.
        builder.HasOne<Client>()
            .WithMany()
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.Name);
        builder.Ignore(x => x.DomainEvents);
    }
}
