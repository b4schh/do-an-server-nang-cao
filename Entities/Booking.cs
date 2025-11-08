namespace FootballField.API.Entities;

// Enums
public enum BookingStatus : byte
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3,
    NoShow = 4
}

public enum PaymentStatus : byte
{
    Unpaid = 0,
    DepositPaid = 1,
    FullyPaid = 2,
    Refunded = 3
}

public class Booking
{
    public int Id { get; set; }
    public int FieldId { get; set; }
    public int CustomerId { get; set; }
    public int OwnerId { get; set; }
    public DateTime BookingDate { get; set; }
    public int TimeSlotId { get; set; }
    public int DepositAmount { get; set; }
    public int TotalAmount { get; set; }
    public BookingStatus BookingStatus { get; set; } = BookingStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    public string? PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public string? Note { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CancelledBy { get; set; }
    public string? CancellationReason { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public bool IsCompleted { get; set; } = false;

    // Navigation properties
    public Field Field { get; set; } = null!;
    public User Customer { get; set; } = null!;
    public User Owner { get; set; } = null!;
    public TimeSlot TimeSlot { get; set; } = null!;
    public User? CancelledByUser { get; set; }
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
