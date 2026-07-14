using FSH.Modules.Cashflow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Cashflow.Data.Configurations;

public sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("Clientes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasColumnName("Nombre").IsRequired().HasMaxLength(256);
        builder.Property(x => x.Code).HasColumnName("Codigo").HasMaxLength(64);
        builder.Property(x => x.TaxId).HasColumnName("NifCif").HasMaxLength(64);
        builder.Property(x => x.Address).HasColumnName("Direccion").HasMaxLength(512);
        builder.Property(x => x.ClientType).HasColumnName("TipoCliente").HasMaxLength(128);
        builder.Property(x => x.Contact).HasColumnName("Contacto").HasMaxLength(256);
        builder.Property(x => x.LegalName).HasColumnName("RazonSocial").HasMaxLength(256);
        builder.Property(x => x.Phone).HasColumnName("Telefono").HasMaxLength(64);
        builder.Property(x => x.Email).HasColumnName("Email").HasMaxLength(256);
        builder.Property(x => x.RegisteredOn).HasColumnName("FechaAlta");
        builder.Property(x => x.ColorHex).HasColumnName("ColorHex").HasMaxLength(32);
        builder.HasIndex(x => x.Name);
        builder.Ignore(x => x.DomainEvents);
    }
}
