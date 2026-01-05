using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoAn.Core.Domain.Entities;

namespace DoAn.Infrastructure.Configurations.System;

public class SystemLogConfiguration : IEntityTypeConfiguration<SystemLog>
{
    public void Configure(EntityTypeBuilder<SystemLog> entity)
    {
        entity.ToTable("SYSTEM_LOG");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id"); // long/bigint

        entity.Property(e => e.LogLevel).HasColumnName("log_level").HasMaxLength(20).IsUnicode(false);
        entity.Property(e => e.Source).HasColumnName("source").HasMaxLength(100).IsUnicode(true);
        entity.Property(e => e.Message).HasColumnName("message").IsUnicode(true);
        entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");
    }
}
