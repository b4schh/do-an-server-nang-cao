namespace FootballField.API.Modules.OwnerSettingsManagement.Dtos
{
    public class CreateOwnerSettingDto
    {
        public int OwnerId { get; set; }
        public decimal? DepositRate { get; set; }
        public int? MinBookingNotice { get; set; }
        public bool AllowReview { get; set; } = true;
    }
}