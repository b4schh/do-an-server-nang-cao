using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DoAn.Core.Domain.Entities;

namespace DoAn.Infrastructure.Configurations.Booking;

public class BookingConfiguration : IEntityTypeConfiguration<DoAn.Core.Domain.Entities.Booking>
{
    public void Configure(EntityTypeBuilder<DoAn.Core.Domain.Entities.Booking> entity)
    {
        entity.ToTable("BOOKING");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");

        entity.Property(e => e.FieldId).HasColumnName("field_id").IsRequired();
        entity.Property(e => e.CustomerId).HasColumnName("customer_id").IsRequired();
        entity.Property(e => e.OwnerId).HasColumnName("owner_id").IsRequired();
        entity.Property(e => e.TimeSlotId).HasColumnName("time_slot_id").IsRequired();
        entity.Property(e => e.BookingDate).HasColumnName("booking_date").IsRequired();
        entity.Property(e => e.HoldExpiresAt).HasColumnName("hold_expires_at").IsRequired();
        entity.Property(e => e.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(10,2)").IsRequired();
        entity.Property(e => e.DepositAmount).HasColumnName("deposit_amount").HasColumnType("decimal(10,2)").IsRequired();
        entity.Property(e => e.PaymentProofUrl).HasColumnName("payment_proof_url").IsUnicode(false);
        entity.Property(e => e.Note).HasColumnName("note").HasMaxLength(255).IsUnicode(true);
        entity.Property(e => e.BookingStatus).HasColumnName("booking_status").IsRequired();
        entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
        entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
        entity.Property(e => e.CancelledBy).HasColumnName("cancelled_by");
        entity.Property(e => e.CancelledAt).HasColumnName("cancelled_at");
        entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");
        entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("DATEADD(HOUR, 7, GETUTCDATE())");

        // CHECK constraints
        entity.ToTable(tb =>
        {
            tb.HasCheckConstraint("CK_Booking_TotalAmount", "total_amount >= 0");
            tb.HasCheckConstraint("CK_Booking_DepositAmount", "deposit_amount >= 0");
            tb.HasCheckConstraint("CK_Booking_DepositLessThanTotal", "deposit_amount <= total_amount");
        });

        // Indexes
        // Filtered unique index: chỉ áp dụng unique cho các trạng thái đang chiếm slot
        // Pending(0), WaitingForApproval(1), Confirmed(2) - các trạng thái này chiếm slot
        // Rejected(3), Cancelled(4), Completed(5), Expired(6), NoShow(7) - không chiếm slot nữa
        entity.HasIndex(e => new { e.FieldId, e.BookingDate, e.TimeSlotId })
            .IsUnique()
            .HasDatabaseName("IX_Booking_UniqueActiveSlot")
            .HasFilter("[booking_status] IN (0, 1, 2)"); // Chỉ unique cho Pending, WaitingForApproval, Confirmed

        entity.HasIndex(e => e.CustomerId).HasDatabaseName("IX_Booking_CustomerId");
        entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_Booking_OwnerId");
        entity.HasIndex(e => new { e.BookingDate, e.BookingStatus }).HasDatabaseName("IX_Booking_BookingDate_Status");

        // Relations
        entity.HasOne(e => e.Field)
            .WithMany(e => e.Bookings)
            .HasForeignKey(e => e.FieldId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(e => e.Customer)
            .WithMany(e => e.CustomerBookings)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(e => e.Owner)
            .WithMany(e => e.OwnerBookings)
            .HasForeignKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(e => e.TimeSlot)
            .WithMany(e => e.Bookings)
            .HasForeignKey(e => e.TimeSlotId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(e => e.ApprovedByUser)
            .WithMany()
            .HasForeignKey(e => e.ApprovedBy)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(e => e.CancelledByUser)
            .WithMany()
            .HasForeignKey(e => e.CancelledBy)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
