using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoAn.Core.Domain.Entities;

namespace DoAn.Infrastructure.Configurations.System;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> entity)
    {
        entity.ToTable("NOTIFICATION");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");

        entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        entity.Property(e => e.SenderId).HasColumnName("sender_id");
        entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(255).IsUnicode(true);
        entity.Property(e => e.Message).HasColumnName("message").IsUnicode(true);
        entity.Property(e => e.Type).HasColumnName("type").IsRequired();
        entity.Property(e => e.RelatedTable).HasColumnName("related_table").HasMaxLength(100).IsUnicode(true);
        entity.Property(e => e.RelatedId).HasColumnName("related_id");
        entity.Property(e => e.IsRead).HasColumnName("is_read").HasDefaultValue(false);
        entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");
        entity.Property(e => e.ReadAt).HasColumnName("read_at");

        entity.HasIndex(e => new { e.UserId, e.IsRead }).HasDatabaseName("IX_Notification_UserId_IsRead");

        entity.HasOne(e => e.User)
            .WithMany(e => e.ReceivedNotifications)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.Sender)
            .WithMany(e => e.SentNotifications)
            .HasForeignKey(e => e.SenderId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
