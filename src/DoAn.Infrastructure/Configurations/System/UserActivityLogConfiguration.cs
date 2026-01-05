using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoAn.Core.Domain.Entities;

namespace DoAn.Infrastructure.Configurations.System;

public class UserActivityLogConfiguration : IEntityTypeConfiguration<UserActivityLog>
{
    public void Configure(EntityTypeBuilder<UserActivityLog> entity)
    {
        entity.ToTable("USER_ACTIVITY_LOG");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id"); // long/bigint

        entity.Property(e => e.UserId).HasColumnName("user_id");
        entity.Property(e => e.Action).HasColumnName("action").HasMaxLength(100).IsUnicode(true);
        entity.Property(e => e.TargetTable).HasColumnName("target_table").HasMaxLength(100).IsUnicode(true);
        entity.Property(e => e.TargetId).HasColumnName("target_id");
        entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500).IsUnicode(true);
        entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");

        entity.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
