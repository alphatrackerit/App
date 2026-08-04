using FSH.Modules.Cashflow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Cashflow.Data.Configurations;

public sealed class ProformaConfiguration : IEntityTypeConfiguration<Proforma>
{
    public void Configure(EntityTypeBuilder<Proforma> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Proformas", t => t.HasCheckConstraint(
            "CK_Proformas_Tipo",
            """("Tipo" = 'Emitida' AND "ClienteId" IS NOT NULL) OR ("Tipo" = 'Recibida' AND "ProveedorId" IS NOT NULL)"""));
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Number).HasColumnName("Numero").IsRequired();
        builder.Property(x => x.Type).HasColumnName("Tipo").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(x => x.Date).HasColumnName("Fecha").HasColumnType("timestamp without time zone");
        builder.Property(x => x.TaxBase).HasColumnName("BaseImponible");
        builder.Property(x => x.Vat).HasColumnName("Iva");
        builder.Property(x => x.Total).HasColumnName("Total").IsRequired();
        builder.Property(x => x.Responsible).HasColumnName("Responsable").HasMaxLength(128);
        builder.Property(x => x.Notes).HasColumnName("Notas");
        builder.Property(x => x.DocumentPath).HasColumnName("Documento").HasMaxLength(512);

        // FormaPago value object → persisted as its Code string; milestones are derived, never stored.
        builder.Property(x => x.PaymentTerms)
            .HasColumnName("FormaPago")
            .HasMaxLength(32)
            .HasConversion(
                vo => vo!.Code,
                code => Domain.PaymentTerms.Parse(code));

        // External soft references — indexed, no FK, consistent with Invoice/Project.
        builder.Property(x => x.ClientId).HasColumnName("ClienteId");
        builder.Property(x => x.SupplierId).HasColumnName("ProveedorId");
        builder.Property(x => x.CompanyId).HasColumnName("EmpresaId");
        builder.Property(x => x.SocietyId).HasColumnName("SociedadId");
        builder.Property(x => x.ProjectId).HasColumnName("ProyectoId");
        builder.Property(x => x.StatusId).HasColumnName("EstadoId");

        builder.HasIndex(x => x.ClientId);
        builder.HasIndex(x => x.SupplierId);
        builder.HasIndex(x => x.CompanyId);
        builder.HasIndex(x => x.SocietyId);
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.StatusId);

        // Same asymmetric duplicate guard as Facturas: Emitida uses OUR numbering → unique per
        // tenant (Finbuckle appends TenantId); Recibida carries the supplier's numbering → lookup
        // index only.
        builder.HasIndex(x => x.Number)
            .IsUnique()
            .HasDatabaseName("IX_Proformas_Numero_Emitida")
            .HasFilter("\"Tipo\" = 'Emitida'");
        builder.HasIndex(x => new { x.Number, x.Type }).HasDatabaseName("IX_Proformas_Numero_Tipo");

        builder.Ignore(x => x.DomainEvents);
    }
}
