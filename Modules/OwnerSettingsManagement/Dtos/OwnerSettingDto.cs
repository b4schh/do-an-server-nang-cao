namespace FootballField.API.Modules.OwnerSettingsManagement.Dtos
{
    public class OwnerSettingDto
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public decimal? DepositRate { get; set; }
        public int? MinBookingNotice { get; set; }
        public bool AllowReview { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}