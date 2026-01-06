using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoAn.Core.Domain.Entities;

namespace DoAn.Infrastructure.Configurations.Auth;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("USER");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");

        entity.Property(e => e.LastName).HasColumnName("last_name").HasMaxLength(100).IsUnicode(true).IsRequired();
        entity.Property(e => e.FirstName).HasColumnName("first_name").HasMaxLength(100).IsUnicode(true).IsRequired();
        entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(200);
        entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(15);
        entity.Property(e => e.Password).HasColumnName("password").HasMaxLength(255);
        entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url").IsUnicode(false);
        entity.Property(e => e.Status).HasColumnName("status").IsRequired();
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
        entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");
        entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");
        entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        // Unique index chỉ áp dụng cho user chưa bị xóa (IsDeleted = false)
        entity.HasIndex(e => e.Email)
            .IsUnique()
            .HasFilter("[is_deleted] = 0");

        // Unique index cho phone, chỉ áp dụng khi phone không null và user chưa bị xóa
        entity.HasIndex(e => e.Phone)
            .IsUnique()
            .HasFilter("[phone] IS NOT NULL AND [is_deleted] = 0");

        entity.HasOne(e => e.DeletedByUser)
            .WithMany()
            .HasForeignKey(e => e.DeletedBy)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(e => e.OwnerSetting)
            .WithOne(e => e.Owner)
            .HasForeignKey<OwnerSetting>(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
