using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoAn.Core.Domain.Entities;

namespace DoAn.Infrastructure.Configurations.Complex;

public class ComplexImageConfiguration : IEntityTypeConfiguration<ComplexImage>
{
    public void Configure(EntityTypeBuilder<ComplexImage> entity)
    {
        entity.ToTable("COMPLEX_IMAGE");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");

        entity.Property(e => e.ComplexId).HasColumnName("complex_id").IsRequired();
        entity.Property(e => e.ImageUrl).HasColumnName("image_url").IsRequired().IsUnicode(false);
        entity.Property(e => e.IsMain).HasColumnName("is_main").HasDefaultValue(false);

        entity.HasOne(e => e.Complex)
            .WithMany(e => e.ComplexImages)
            .HasForeignKey(e => e.ComplexId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
