using FSH.Framework.Shared.Multitenancy;
using FSH.Modules.Multitenancy.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Multitenancy.Data.Configurations;

public sealed class TenantEmailDomainConfiguration : IEntityTypeConfiguration<TenantEmailDomain>
{
    public void Configure(EntityTypeBuilder<TenantEmailDomain> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("TenantEmailDomains", MultitenancyConstants.Schema);

        builder.HasKey(d => d.Id);

        // 253 = max DNS name length. Unique so a domain resolves to exactly one tenant.
        builder.Property(d => d.Domain).HasMaxLength(253).IsRequired();
        builder.Property(d => d.TenantId).HasMaxLength(64).IsRequired();

        builder.HasIndex(d => d.Domain).IsUnique();
        builder.HasIndex(d => d.TenantId);
    }
}
