using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoAn.Core.Domain.Entities;

namespace DoAn.Infrastructure.Configurations.Review;

public class ReviewImageConfiguration : IEntityTypeConfiguration<ReviewImage>
{
    public void Configure(EntityTypeBuilder<ReviewImage> entity)
    {
        entity.ToTable("REVIEW_IMAGE");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");

        entity.Property(e => e.ReviewId).HasColumnName("review_id").IsRequired();
        entity.Property(e => e.ImageUrl).HasColumnName("image_url").IsRequired().IsUnicode(false);
        entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");

        entity.HasIndex(e => e.ReviewId).HasDatabaseName("IX_ReviewImage_ReviewId");

        entity.HasOne(e => e.Review)
            .WithMany(e => e.Images)
            .HasForeignKey(e => e.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
