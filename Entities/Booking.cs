namespace FootballField.API.Entities;

// Enums
public enum BookingStatus : byte
{
    Pending = 0,              // Khách vừa tạo booking, chưa upload bill
    WaitingForApproval = 1,   // Khách đã upload bill, chờ chủ sân duyệt
    Confirmed = 2,            // Chủ sân đã duyệt cọc, giữ sân thành công
    Rejected = 3,             // Chủ sân từ chối bill
    Cancelled = 4,            // Khách hoặc chủ sân hủy booking
    Completed = 5,            // Trận đấu đã diễn ra, thanh toán đầy đủ
    Expired = 6,              // Hết thời gian giữ chỗ, khách không upload bill
    NoShow = 7                // Khách không đến sân
}

public class Booking
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public int CustomerId { get; set; }
    public int OwnerId { get; set; }
    public int TimeSlotId { get; set; }
    public DateTime BookingDate { get; set; }
    public DateTime HoldExpiresAt { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DepositAmount { get; set; }
    public string? PaymentProofUrl { get; set; }
    public string? Note { get; set; }
    public BookingStatus BookingStatus { get; set; } = BookingStatus.Pending;
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? CancelledBy { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Field Field { get; set; } = null!;
    public User Customer { get; set; } = null!;
    public User Owner { get; set; } = null!;
    public TimeSlot TimeSlot { get; set; } = null!;
    public User? ApprovedByUser { get; set; }
    public User? CancelledByUser { get; set; }
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
