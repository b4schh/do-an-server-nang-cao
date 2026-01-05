using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoAn.Core.Domain.Entities;

namespace DoAn.Infrastructure.Configurations.System;

public class SystemConfigConfiguration : IEntityTypeConfiguration<SystemConfig>
{
    public void Configure(EntityTypeBuilder<SystemConfig> entity)
    {
        entity.ToTable("SYSTEM_CONFIG");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");

        entity.Property(e => e.ConfigKey).HasColumnName("config_key").HasMaxLength(100).IsUnicode(true).IsRequired();
        entity.Property(e => e.ConfigValue).HasColumnName("config_value").HasMaxLength(255).IsUnicode(true);
        entity.Property(e => e.DataType).HasColumnName("data_type").HasMaxLength(20).HasDefaultValue("string").IsUnicode(false);
        entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(255).IsUnicode(true);
        entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");

        entity.HasIndex(e => e.ConfigKey).IsUnique();

        entity.ToTable(tb =>
        {
            tb.HasCheckConstraint("CK_SystemConfig_DataType", "data_type IN ('string', 'int', 'decimal', 'boolean', 'datetime')");
        });
    }
}
