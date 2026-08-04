using FSH.Modules.Cashflow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Cashflow.Data.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Facturas", t => t.HasCheckConstraint(
            "CK_Facturas_Tipo",
            """("Tipo" = 'Emitida' AND "ClienteId" IS NOT NULL) OR ("Tipo" = 'Recibida' AND "ProveedorId" IS NOT NULL)"""));
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Number).HasColumnName("Numero").IsRequired();
        builder.Property(x => x.DynamicsNumber).HasColumnName("NumeroDynamics");
        builder.Property(x => x.Type).HasColumnName("Tipo").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(x => x.InvoiceDate).HasColumnName("FechaFactura").HasColumnType("timestamp without time zone");
        builder.Property(x => x.DueDate).HasColumnName("FechaVencimiento").HasColumnType("timestamp without time zone");
        builder.Property(x => x.TaxBase).HasColumnName("BaseImponible");
        builder.Property(x => x.Vat).HasColumnName("Iva");
        builder.Property(x => x.Total).HasColumnName("Total").IsRequired();
        builder.Property(x => x.Bank).HasColumnName("Banco");
        builder.Property(x => x.Verified).HasColumnName("Comprobada").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.VerifactuStatus)
            .HasColumnName("VerifactuEstado")
            .HasConversion<string>()
            .HasMaxLength(24)
            .IsRequired()
            .HasDefaultValue(Contracts.Enums.VerifactuStatus.NoAplica);
        builder.Property(x => x.Notes).HasColumnName("Notas");
        builder.Property(x => x.DocumentPath).HasColumnName("Documento").HasMaxLength(512);

        // FormaPago value object → persisted as its Code string (§5); milestones are derived, never stored.
        builder.Property(x => x.PaymentTerms)
            .HasColumnName("FormaPago")
            .HasMaxLength(32)
            .HasConversion(
                vo => vo!.Code,
                code => Domain.PaymentTerms.Parse(code));

        // External soft references (Clientes / Proveedores / Empresas / Sociedades / Estados) — indexed, no FK,
        // consistent with Project. Only the cash-line FK (Ingresos/Pagos → Facturas) is a real database FK.
        builder.Property(x => x.ClientId).HasColumnName("ClienteId");
        builder.Property(x => x.SupplierId).HasColumnName("ProveedorId");
        builder.Property(x => x.CompanyId).HasColumnName("EmpresaId");
        builder.Property(x => x.SocietyId).HasColumnName("SociedadId");
        builder.Property(x => x.ProjectId).HasColumnName("ProyectoId");
        builder.Property(x => x.StatusId).HasColumnName("EstadoId");

        // Conceptos: owned child rows, real FK, die with the invoice. Backing-field access so
        // SetItems' clear+add round-trips correctly.
        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Proforma link — real intra-module FK (like Income/Payment → Invoice): deleting the
        // proforma unlinks its invoices instead of losing them.
        builder.Property(x => x.ProformaId).HasColumnName("ProformaId");
        builder.HasOne<Proforma>()
            .WithMany()
            .HasForeignKey(x => x.ProformaId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => x.ProformaId);

        builder.HasIndex(x => x.ClientId);
        builder.HasIndex(x => x.SupplierId);
        builder.HasIndex(x => x.CompanyId);
        builder.HasIndex(x => x.SocietyId);
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.StatusId);

        // Duplicate guard (§8) — asymmetric by design, learned from the real 2026 ledgers.
        // Emitida uses OUR numbering → unique per tenant via a partial index — Finbuckle appends
        // TenantId. Recibida carries the SUPPLIER'S numbering — different suppliers legitimately
        // reuse the same number and charge/refund pairs share one number, so uniqueness is NOT
        // enforced there. Dedupe belongs to the importer, not the schema — lookup index only.
        builder.HasIndex(x => x.Number)
            .IsUnique()
            .HasDatabaseName("IX_Facturas_Numero_Emitida")
            .HasFilter("\"Tipo\" = 'Emitida'");
        builder.HasIndex(x => new { x.Number, x.Type }).HasDatabaseName("IX_Facturas_Numero_Tipo");

        builder.Ignore(x => x.DomainEvents);
    }
}
