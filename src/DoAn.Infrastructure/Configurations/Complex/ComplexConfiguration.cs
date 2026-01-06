using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoAn.Core.Domain.Entities;

namespace DoAn.Infrastructure.Configurations.Complex;

public class ComplexConfiguration : IEntityTypeConfiguration<DoAn.Core.Domain.Entities.Complex>
{
    public void Configure(EntityTypeBuilder<DoAn.Core.Domain.Entities.Complex> entity)
    {
        entity.ToTable("COMPLEX");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");

        entity.Property(e => e.OwnerId).HasColumnName("owner_id").IsRequired();
        entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsUnicode(true).IsRequired();
        entity.Property(e => e.Street).HasColumnName("street").HasMaxLength(100).IsUnicode(true);
        entity.Property(e => e.Ward).HasColumnName("ward").HasMaxLength(100).IsUnicode(true);
        entity.Property(e => e.Province).HasColumnName("province").HasMaxLength(100).IsUnicode(true);
        entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(15);
        entity.Property(e => e.OpeningTime).HasColumnName("opening_time");
        entity.Property(e => e.ClosingTime).HasColumnName("closing_time");
        entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500).IsUnicode(true);
        entity.Property(e => e.Status).HasColumnName("status").IsRequired();
        entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(500).IsUnicode(true);
        entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");
        entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");
        entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_Complex_OwnerId");

        entity.HasOne(e => e.Owner)
            .WithMany(e => e.OwnedComplexes)
            .HasForeignKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
