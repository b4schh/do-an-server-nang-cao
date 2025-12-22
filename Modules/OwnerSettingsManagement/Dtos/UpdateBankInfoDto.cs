using Microsoft.AspNetCore.Http;

namespace FootballField.API.Modules.OwnerSettingsManagement.Dtos
{
    public class UpdateBankInfoDto
    {
        public string BankName { get; set; } = null!;
        public string BankAccountNumber { get; set; } = null!;
        public string BankAccountName { get; set; } = null!;
        public IFormFile? QrCodeImage { get; set; }
    }
}
