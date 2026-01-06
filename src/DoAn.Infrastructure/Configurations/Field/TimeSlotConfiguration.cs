using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoAn.Core.Domain.Entities;

namespace DoAn.Infrastructure.Configurations.Field;

public class TimeSlotConfiguration : IEntityTypeConfiguration<TimeSlot>
{
    public void Configure(EntityTypeBuilder<TimeSlot> entity)
    {
        entity.ToTable("TIME_SLOT");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");

        entity.Property(e => e.FieldId).HasColumnName("field_id").IsRequired();
        entity.Property(e => e.StartTime).HasColumnName("start_time").IsRequired();
        entity.Property(e => e.EndTime).HasColumnName("end_time").IsRequired();
        entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(10,2)").IsRequired();
        entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");
        entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");

        entity.HasIndex(e => e.FieldId).HasDatabaseName("IX_TimeSlot_FieldId");
        entity.HasIndex(e => new { e.FieldId, e.StartTime, e.EndTime }).IsUnique();

        // CHECK: start_time < end_time
        entity.ToTable(tb =>
        {
            tb.HasCheckConstraint("CK_TimeSlot_TimeRange", "start_time < end_time");
        });

        entity.HasOne(e => e.Field)
            .WithMany(e => e.TimeSlots)
            .HasForeignKey(e => e.FieldId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
