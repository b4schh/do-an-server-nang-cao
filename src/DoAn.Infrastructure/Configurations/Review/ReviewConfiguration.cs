using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoAn.Core.Domain.Entities;

namespace DoAn.Infrastructure.Configurations.Review;

public class ReviewConfiguration : IEntityTypeConfiguration<DoAn.Core.Domain.Entities.Review>
{
    public void Configure(EntityTypeBuilder<DoAn.Core.Domain.Entities.Review> entity)
    {
        entity.ToTable("REVIEW");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");

        entity.Property(e => e.BookingId).HasColumnName("booking_id").IsRequired();
        entity.Property(e => e.Rating).HasColumnName("rating").IsRequired();
        entity.Property(e => e.Comment).HasColumnName("comment").HasMaxLength(1000).IsUnicode(true);
        entity.Property(e => e.IsVisible).HasColumnName("is_visible").HasDefaultValue(true);
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");
        entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");
        entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        entity.ToTable(tb =>
        {
            tb.HasCheckConstraint("CK_Review_Rating", "rating >= 1 AND rating <= 5");
        });

        entity.HasOne(e => e.Booking)
            .WithMany(e => e.Reviews)
            .HasForeignKey(e => e.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
