using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoAn.Core.Domain.Entities;

namespace DoAn.Infrastructure.Configurations.Complex;

public class FavoriteComplexConfiguration : IEntityTypeConfiguration<FavoriteComplex>
{
    public void Configure(EntityTypeBuilder<FavoriteComplex> entity)
    {
        entity.ToTable("FAVORITE_COMPLEX");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");

        entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        entity.Property(e => e.ComplexId).HasColumnName("complex_id").IsRequired();
        entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");

        entity.HasIndex(e => new { e.UserId, e.ComplexId }).IsUnique();

        entity.HasOne(e => e.User)
            .WithMany(e => e.FavoriteComplexes)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.Complex)
            .WithMany()
            .HasForeignKey(e => e.ComplexId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
