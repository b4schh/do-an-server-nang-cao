using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoAn.Core.Domain.Entities;

namespace DoAn.Infrastructure.Configurations.Auth;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> entity)
    {
        entity.ToTable("PERMISSION");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");

        entity.Property(e => e.PermissionKey).HasColumnName("permission_key").HasMaxLength(100).IsRequired();
        entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(255).IsUnicode(true);
        entity.Property(e => e.Module).HasColumnName("module").HasMaxLength(50).IsUnicode(true);
        entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");
        entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");

        entity.HasIndex(e => e.PermissionKey).IsUnique();
    }
}
