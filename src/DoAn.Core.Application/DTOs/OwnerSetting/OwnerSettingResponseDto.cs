namespace DoAn.Core.Application.DTOs.OwnerSetting;

public class OwnerSettingResponseDto
{
    public OwnerSettingDto? Data { get; set; }
    public SystemDefaultsDto SystemDefaults { get; set; } = null!;
}

public class SystemDefaultsDto
{
    public decimal DepositRate { get; set; }
    public int MinBookingNotice { get; set; }
    public bool AllowReview { get; set; }
}
