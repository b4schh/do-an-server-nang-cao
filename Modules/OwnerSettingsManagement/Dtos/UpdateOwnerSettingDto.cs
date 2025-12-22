namespace FootballField.API.Modules.OwnerSettingsManagement.Dtos
{
    public class UpdateOwnerSettingDto
    {
        public decimal? DepositRate { get; set; }
        public int? MinBookingNotice { get; set; }
        public bool AllowReview { get; set; }
        
        // Bank information (nullable - owner can update separately)
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankAccountName { get; set; }
    }
}