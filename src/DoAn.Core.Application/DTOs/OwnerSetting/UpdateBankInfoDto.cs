using Microsoft.AspNetCore.Http;

namespace DoAn.Core.Application.DTOs.OwnerSetting;

public class UpdateBankInfoDto
{
    public string BankName { get; set; } = null!;
    public string BankAccountNumber { get; set; } = null!;
    public string BankAccountName { get; set; } = null!;
    public IFormFile? QrCodeImage { get; set; }
}
