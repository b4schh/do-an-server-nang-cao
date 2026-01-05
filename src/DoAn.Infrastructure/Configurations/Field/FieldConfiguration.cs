using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoAn.Core.Domain.Entities;

namespace DoAn.Infrastructure.Configurations.Field;

public class FieldConfiguration : IEntityTypeConfiguration<DoAn.Core.Domain.Entities.Field>
{
    public void Configure(EntityTypeBuilder<DoAn.Core.Domain.Entities.Field> entity)
    {
        entity.ToTable("FIELD");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");

        entity.Property(e => e.ComplexId).HasColumnName("complex_id").IsRequired();
        entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsUnicode(true).IsRequired();
        entity.Property(e => e.SurfaceType).HasColumnName("surface_type").HasMaxLength(50).IsUnicode(true);
        entity.Property(e => e.FieldSize).HasColumnName("field_size").HasMaxLength(50).IsUnicode(true);
        entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");
        entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");
        entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        entity.HasIndex(e => e.ComplexId).HasDatabaseName("IX_Field_ComplexId");

        entity.HasOne(e => e.Complex)
            .WithMany(e => e.Fields)
            .HasForeignKey(e => e.ComplexId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
