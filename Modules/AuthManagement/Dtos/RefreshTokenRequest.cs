using System.ComponentModel.DataAnnotations;

namespace FootballField.API.Modules.AuthManagement.Dtos
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Refresh token không được để trống")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
