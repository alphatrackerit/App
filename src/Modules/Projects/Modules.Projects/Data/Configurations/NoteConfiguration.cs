using FSH.Modules.Projects.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Projects.Data.Configurations;

public sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Notas");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProjectId).HasColumnName("ProyectoId");
        builder.Property(x => x.Title).HasColumnName("Titulo").IsRequired();
        builder.Property(x => x.Description).HasColumnName("Descripcion");
        builder.Property(x => x.Date)
            .HasColumnName("Fecha")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("'-infinity'");

        // Owning project — same module/schema → real FK (ON DELETE NO ACTION). EF adds the FK index.
        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Ignore(x => x.DomainEvents);
    }
}
