namespace FootballField.API.Modules.OwnerSettingsManagement.Dtos
{
    public class UpdateOwnerSettingDto
    {
        public decimal? DepositRate { get; set; }
        public int? MinBookingNotice { get; set; }
        public bool AllowReview { get; set; }
    }
}