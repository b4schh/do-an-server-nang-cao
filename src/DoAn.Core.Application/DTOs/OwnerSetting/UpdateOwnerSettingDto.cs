namespace DoAn.Core.Application.DTOs.OwnerSetting;

public class UpdateOwnerSettingDto
{
    public decimal? DepositRate { get; set; }
    public int? MinBookingNotice { get; set; }
    public bool AllowReview { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankAccountName { get; set; }
}