using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoAn.Core.Domain.Entities;

namespace DoAn.Infrastructure.Configurations.Auth;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> entity)
    {
        entity.ToTable("ROLE");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");

        entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(50).IsUnicode(true).IsRequired();
        entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(255).IsUnicode(true);
        entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");
        entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");

        entity.HasIndex(e => e.Name).IsUnique();
    }
}
