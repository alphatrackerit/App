using FSH.Modules.Avicola.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Avicola.Data.Configurations;

public sealed class DocumentoConfiguration : IEntityTypeConfiguration<Documento>
{
    public void Configure(EntityTypeBuilder<Documento> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Documentos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Tipo).HasColumnName("Tipo").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Origen).HasColumnName("Origen").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.OrigenId).HasColumnName("OrigenId");
        builder.Property(x => x.FileAssetId).HasColumnName("FileAssetId");
        builder.Property(x => x.Url).HasColumnName("Url").IsRequired();
        builder.Property(x => x.NombreArchivo).HasColumnName("NombreArchivo").IsRequired();
        builder.Property(x => x.ContentType).HasColumnName("ContentType");
        builder.Property(x => x.Fecha).HasColumnName("Fecha").HasColumnType("timestamp with time zone");
        builder.Property(x => x.Notas).HasColumnName("Notas");

        // Polymorphic owner (no FK). Indexed for "documents of <entity>" lookups.
        builder.HasIndex(x => new { x.Origen, x.OrigenId });

        builder.Ignore(x => x.DomainEvents);
    }
}
