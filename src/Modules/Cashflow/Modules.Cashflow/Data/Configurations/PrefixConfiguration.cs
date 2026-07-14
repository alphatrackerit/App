using FSH.Modules.Cashflow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Cashflow.Data.Configurations;

public sealed class PrefixConfiguration : IEntityTypeConfiguration<Prefix>
{
    public void Configure(EntityTypeBuilder<Prefix> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("Prefijos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasColumnName("Nombre").IsRequired().HasMaxLength(256);
        builder.Property(x => x.Description).HasColumnName("Descripcion").HasMaxLength(1024);
        builder.Property(x => x.PrefixGroupId).HasColumnName("PrefijoGrupoId");
        builder.Property(x => x.Type).HasColumnName("Tipo").HasConversion<string>().IsRequired().HasMaxLength(16);
        builder.Property(x => x.IsActive).HasColumnName("Activo").IsRequired().HasDefaultValue(true);

        // Self-reference: a CATEGORIA points at its GRUPO (ON DELETE NO ACTION).
        builder.HasOne<Prefix>()
            .WithMany()
            .HasForeignKey(x => x.PrefixGroupId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.Type);
        builder.Ignore(x => x.DomainEvents);
    }
}
