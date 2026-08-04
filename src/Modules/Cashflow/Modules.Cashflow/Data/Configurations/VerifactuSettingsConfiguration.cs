using FSH.Modules.Cashflow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Cashflow.Data.Configurations;

public sealed class VerifactuSettingsConfiguration : IEntityTypeConfiguration<VerifactuSettings>
{
    public void Configure(EntityTypeBuilder<VerifactuSettings> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("VerifactuAjustes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CompanyId).HasColumnName("EmpresaId").IsRequired();
        builder.Property(x => x.Environment).HasColumnName("Entorno").HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(x => x.Enabled).HasColumnName("Activo").IsRequired().HasDefaultValue(false);
        builder.Property(x => x.SoftwareName).HasColumnName("NombreSoftware").HasMaxLength(128);
        builder.Property(x => x.SoftwareVersion).HasColumnName("VersionSoftware").HasMaxLength(64);
        builder.Property(x => x.InstallationNumber).HasColumnName("NumeroInstalacion").HasMaxLength(64);
        builder.Property(x => x.EncryptedCertificate).HasColumnName("CertificadoCifrado");
        builder.Property(x => x.EncryptedCertificatePassword).HasColumnName("PasswordCertificadoCifrado").HasMaxLength(2048);
        builder.Property(x => x.LastChainHash).HasColumnName("UltimaHuella").HasMaxLength(64);
        builder.Property(x => x.LastChainAt).HasColumnName("UltimaHuellaFecha");

        // Postgres system column xmin as the optimistic-concurrency token.
        builder.Property(x => x.RowVersion).IsRowVersion();

        // 1:1 with Company — real intra-module FK (same pattern as Society→Client). Deleting a
        // company with a VeriFactu chain is refused by the database (NO ACTION), on purpose.
        builder.HasOne<Company>()
            .WithOne()
            .HasForeignKey<VerifactuSettings>(x => x.CompanyId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(x => x.CompanyId).IsUnique();

        builder.Ignore(x => x.DomainEvents);
    }
}
