using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoAn.Core.Domain.Entities;

namespace DoAn.Infrastructure.Configurations.Auth;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> entity)
    {
        entity.ToTable("ROLE_PERMISSION");

        entity.HasKey(e => new { e.RoleId, e.PermissionId });

        entity.Property(e => e.RoleId).HasColumnName("role_id");
        entity.Property(e => e.PermissionId).HasColumnName("permission_id");
        entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");

        entity.HasOne(e => e.Role)
            .WithMany(e => e.RolePermissions)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.Permission)
            .WithMany(e => e.RolePermissions)
            .HasForeignKey(e => e.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
