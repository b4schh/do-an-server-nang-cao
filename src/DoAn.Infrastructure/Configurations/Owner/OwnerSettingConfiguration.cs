using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoAn.Core.Domain.Entities;

namespace DoAn.Infrastructure.Configurations.Owner;

public class OwnerSettingConfiguration : IEntityTypeConfiguration<OwnerSetting>
{
    public void Configure(EntityTypeBuilder<OwnerSetting> entity)
    {
        entity.ToTable("OWNER_SETTING");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");

        entity.Property(e => e.OwnerId).HasColumnName("owner_id").IsRequired();
        entity.Property(e => e.DepositRate).HasColumnName("deposit_rate").HasColumnType("decimal(5,2)");
        entity.Property(e => e.MinBookingNotice).HasColumnName("min_booking_notice");
        entity.Property(e => e.AllowReview).HasColumnName("allow_review").HasDefaultValue(true);

        // Bank information fields
        entity.Property(e => e.BankName).HasColumnName("bank_name").HasMaxLength(100).IsUnicode(true);
        entity.Property(e => e.BankAccountNumber).HasColumnName("bank_account_number").HasMaxLength(50).IsUnicode(false);
        entity.Property(e => e.BankAccountName).HasColumnName("bank_account_name").HasMaxLength(200).IsUnicode(true);
        entity.Property(e => e.BankQrCodeUrl).HasColumnName("bank_qr_code_url").IsUnicode(false);

        entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");
        entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");

        entity.HasIndex(e => e.OwnerId).IsUnique();

        // CHECKs
        entity.ToTable(tb =>
        {
            tb.HasCheckConstraint("CK_OwnerSetting_DepositRate", "deposit_rate >= 0 AND deposit_rate <= 100");
            tb.HasCheckConstraint("CK_OwnerSetting_MinBookingNotice", "min_booking_notice >= 0");
        });
    }
}
