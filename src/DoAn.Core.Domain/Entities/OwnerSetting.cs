namespace DoAn.Core.Domain.Entities;

public class OwnerSetting
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    
    // Booking settings (override SystemConfig if not null)
    public decimal? DepositRate { get; set; }
    public int? MinBookingNotice { get; set; }
    public bool AllowReview { get; set; } = true;
    
    // Bank information for payment
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankAccountName { get; set; }
    public string? BankQrCodeUrl { get; set; }  // URL ảnh QR code trên MinIO
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public User Owner { get; set; } = null!;
}