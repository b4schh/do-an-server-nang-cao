namespace FootballField.API.Dtos.OwnerSetting
{
    public class CreateOwnerSettingDto
    {
        public int OwnerId { get; set; }
        public decimal? DepositRate { get; set; }
        public int? MinBookingNotice { get; set; }
        public bool AllowReview { get; set; } = true;
    }
}
