using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoAn.Core.Domain.Entities;

namespace DoAn.Infrastructure.Configurations.Review;

public class ReviewHelpfulVoteConfiguration : IEntityTypeConfiguration<ReviewHelpfulVote>
{
    public void Configure(EntityTypeBuilder<ReviewHelpfulVote> entity)
    {
        entity.ToTable("REVIEW_HELPFUL_VOTE");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");

        entity.Property(e => e.ReviewId).HasColumnName("review_id").IsRequired();
        entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");

        entity.HasIndex(e => new { e.ReviewId, e.UserId }).IsUnique().HasDatabaseName("IX_ReviewHelpfulVote_ReviewId_UserId");

        entity.HasOne(e => e.Review)
            .WithMany(e => e.HelpfulVotes)
            .HasForeignKey(e => e.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
