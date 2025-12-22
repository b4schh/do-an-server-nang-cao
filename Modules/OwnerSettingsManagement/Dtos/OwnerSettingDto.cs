namespace FootballField.API.Modules.OwnerSettingsManagement.Dtos
{
    public class OwnerSettingDto
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public decimal? DepositRate { get; set; }
        public int? MinBookingNotice { get; set; }
        public bool AllowReview { get; set; }
        
        // Bank information
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankAccountName { get; set; }
        public string? BankQrCodeUrl { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}