using FSH.Modules.Avicola.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Avicola.Data.Configurations;

public sealed class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Pedidos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Codigo).HasColumnName("Codigo").IsRequired();
        builder.Property(x => x.Tipo).HasColumnName("Tipo").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.ProveedorId).HasColumnName("ProveedorId");
        builder.Property(x => x.Descripcion).HasColumnName("Descripcion");
        builder.Property(x => x.Cantidad).HasColumnName("Cantidad");
        builder.Property(x => x.Unidad).HasColumnName("Unidad");
        builder.Property(x => x.CostoEstimado).HasColumnName("CostoEstimado");
        builder.Property(x => x.CostoReal).HasColumnName("CostoReal");
        builder.Property(x => x.Estado).HasColumnName("Estado").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.FechaPedido).HasColumnName("FechaPedido").HasColumnType("timestamp with time zone");
        builder.Property(x => x.FechaRecepcion).HasColumnName("FechaRecepcion").HasColumnType("timestamp with time zone");
        builder.Property(x => x.LoteId).HasColumnName("LoteId");
        builder.Property(x => x.GalponId).HasColumnName("GalponId");
        builder.Property(x => x.Notas).HasColumnName("Notas");

        builder.HasOne<Lote>().WithMany().HasForeignKey(x => x.LoteId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<Galpon>().WithMany().HasForeignKey(x => x.GalponId).OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(x => x.ProveedorId);
        builder.HasIndex(x => x.Estado);

        builder.Ignore(x => x.DomainEvents);
    }
}
