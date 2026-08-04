using FSH.Modules.Cashflow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Cashflow.Data.Configurations;

public sealed class InvoiceVerifactuRecordConfiguration : IEntityTypeConfiguration<InvoiceVerifactuRecord>
{
    public void Configure(EntityTypeBuilder<InvoiceVerifactuRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("VerifactuRegistros");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.InvoiceId).HasColumnName("FacturaId").IsRequired();
        builder.Property(x => x.PreviousHash).HasColumnName("HuellaAnterior").HasMaxLength(64);
        builder.Property(x => x.Hash).HasColumnName("Huella").HasMaxLength(64).IsRequired();
        builder.Property(x => x.GeneratedAt).HasColumnName("FechaGeneracion").IsRequired();
        builder.Property(x => x.QrPayload).HasColumnName("QrPayload").HasMaxLength(512).IsRequired();
        builder.Property(x => x.Status).HasColumnName("Estado").HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(x => x.AeatResponseCode).HasColumnName("CodigoRespuestaAeat").HasMaxLength(64);
        builder.Property(x => x.RawRequestXml).HasColumnName("PeticionXml");
        builder.Property(x => x.RawResponseXml).HasColumnName("RespuestaXml");
        builder.Property(x => x.RetryCount).HasColumnName("Reintentos").IsRequired().HasDefaultValue(0);

        // 1:1 with Invoice — real intra-module FK. RESTRICT: a registered invoice can never be
        // deleted (the domain guard 409s first; the FK is the backstop).
        builder.HasOne<Invoice>()
            .WithOne()
            .HasForeignKey<InvoiceVerifactuRecord>(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.InvoiceId).IsUnique();
        builder.HasIndex(x => x.Status);

        builder.Ignore(x => x.DomainEvents);
    }
}
