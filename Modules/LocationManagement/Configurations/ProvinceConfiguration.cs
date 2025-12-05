using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FootballField.API.Modules.LocationManagement.Entities;

namespace FootballField.API.Modules.LocationManagement.Configurations;

public class ProvinceConfiguration : IEntityTypeConfiguration<Province>
{
    public void Configure(EntityTypeBuilder<Province> entity)
    {
        entity.ToTable("Provinces", "location");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        entity.Property(e => e.Code)
            .HasColumnName("code")
            .IsRequired();

        entity.HasIndex(e => e.Code)
            .IsUnique()
            .HasDatabaseName("IX_Province_Code");

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

        entity.HasIndex(e => e.Name);
    }
}
