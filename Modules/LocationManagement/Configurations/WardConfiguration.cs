using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FootballField.API.Modules.LocationManagement.Entities;

namespace FootballField.API.Modules.LocationManagement.Configurations;

public class WardConfiguration : IEntityTypeConfiguration<Ward>
{
    public void Configure(EntityTypeBuilder<Ward> entity)
    {
        entity.ToTable("Wards", "location");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        entity.Property(e => e.Code)
            .HasColumnName("code")
            .IsRequired();

        entity.HasIndex(e => e.Code)
            .IsUnique()
            .HasDatabaseName("IX_Ward_Code");

        entity.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsUnicode(true)
            .IsRequired();

        entity.Property(e => e.Codename)
            .HasColumnName("codename")
            .HasMaxLength(100)
            .IsUnicode(true)
            .IsRequired();

        entity.Property(e => e.DivisionType)
            .HasColumnName("division_type")
            .HasMaxLength(50)
            .IsUnicode(true)
            .IsRequired();

        entity.Property(e => e.ProvinceCode)
            .HasColumnName("province_code")
            .IsRequired();

        entity.HasIndex(e => e.ProvinceCode).HasDatabaseName("IX_Ward_ProvinceCode");
        entity.HasIndex(e => e.Name);

        entity.HasOne(e => e.Province)
            .WithMany(e => e.Wards)
            .HasForeignKey(e => e.ProvinceCode)
            .HasPrincipalKey(e => e.Code)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
